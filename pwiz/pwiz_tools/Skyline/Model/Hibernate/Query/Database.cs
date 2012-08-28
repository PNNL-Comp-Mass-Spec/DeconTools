﻿/*
 * Original author: Nick Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Lib;
using pwiz.Skyline.Model.Results;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model.Hibernate.Query
{
    /// <summary>
    /// In-memory SQLite database that holds all of the queryable information in the document.
    /// </summary>
    public class Database : IDisposable
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly ISession _session;
        private DataSettings _dataSettings;
        private bool _isDisposed;

        public Database()
            : this(null)
        {            
        }

        public Database(SrmSettings settings)
        {
            var configuration = SessionFactoryFactory.GetConfiguration(":memory:", settings);
            // In-memory SQLite databases disappear the moment that you release the connection,
            // so we have to tell Hibernate not to release the connection until we close the 
            // session.
            configuration.SetProperty("connection.release_mode", "on_close");
            _sessionFactory = configuration.BuildSessionFactory();
            _session = _sessionFactory.OpenSession();
            new SchemaExport(configuration).Execute(false, true, false, _session.Connection, null);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _sessionFactory.Dispose();
                _session.Dispose();
            }
        }

        public ILongWaitBroker LongWaitBroker { get; set; }
        public int PercentOfWait { get; set; }

        public ISessionFactory SessionFactory
        {
            get { return _sessionFactory; }
        }

        public Schema GetSchema()
        {
            return new Schema(SessionFactory, _dataSettings);
        }

        public ResultSet ExecuteQuery(IList<ReportColumn> columns)
        {
            Schema schema = new Schema(SessionFactory, _dataSettings);
            StringBuilder hql = new StringBuilder("SELECT ");
            String comma = "";
            var columnInfos = new List<ColumnInfo>();
            var dictTableAlias = new Dictionary<Type, string>();
            foreach (ReportColumn column in columns)
            {
                hql.Append(comma);
                comma = ", ";
                hql.Append(column.GetHql(dictTableAlias));

                columnInfos.Add(schema.GetColumnInfo(column));
            }
            hql.Append("\nFROM ");
            comma = "";
            var listTableAlias = new List<KeyValuePair<Type, string>>(ReportColumn.Order(dictTableAlias));
            foreach (var tableAlias in listTableAlias)
            {
                hql.Append(comma);
                comma = ", ";
                hql.Append(tableAlias.Key);
                hql.Append(" ");
                hql.Append(tableAlias.Value);
            }
            if (dictTableAlias.Count > 1)
            {
                hql.Append("\nWHERE ");
                string and = "";
                for (int i = 0; i < listTableAlias.Count - 1; i++)
                {
                    for (int j = i + 1; j < listTableAlias.Count; j++)
                    {
                        hql.Append(and);
                        and = " AND\n";
                        Join(hql, listTableAlias[i], listTableAlias[j]);
                    }
                }
            }
            IQuery query = Session.CreateQuery(hql.ToString());
            return new ResultSet(columnInfos, query.List());
        }

        private static void Join(StringBuilder hql, KeyValuePair<Type, string> table1, KeyValuePair<Type, string> table2)
        {
            TableType tableType1 = ReportColumn.GetTableType(table1.Key);
            TableType tableType2 = ReportColumn.GetTableType(table2.Key);
            if (tableType1 == tableType2)
                throw new InvalidOperationException("Cannot join tables of same type.");
            if (tableType2 == TableType.node)
            {
                Helpers.Swap(ref table1, ref table2);
                Helpers.Swap(ref tableType1, ref tableType2);
            }
            if (tableType1 == TableType.node)
            {
                // Join node Id to the node column Id in the result table
                // Node Id
                hql.Append(table1.Value);
                hql.Append(".Id = ");
                // Result node Id
                hql.Append(table2.Value);
                hql.Append(".");
                hql.Append(GetJoinColumn(table2, table1));
                hql.Append(".Id");
            }
            else
            {
                if (tableType2 == TableType.result)
                    Helpers.Swap(ref table1, ref table2);
                // Join all entries for which their replicate path starts with the summary path
                hql.Append("substring(");
                // Result replicate path
                hql.Append(table1.Value);
                hql.Append(".ResultFile.Replicate.ReplicatePath, 1, length(");
                // Summary path
                hql.Append(table2.Value);
                hql.Append(".ReplicatePath)) = ");
                hql.Append(table2.Value);
                hql.Append(".ReplicatePath");
            }
        }

        private static string GetJoinColumn(KeyValuePair<Type, string> table2, KeyValuePair<Type, string> table1)
        {
            MemberInfo[] memberInfo = table2.Key.FindMembers(MemberTypes.Property,
                BindingFlags.Public | BindingFlags.Instance,
                (info, o) => Equals(((PropertyInfo)info).PropertyType, o),
                table1.Key);

            if (memberInfo.Length < 1)
                throw new InvalidOperationException(string.Format("The type {0} must have a column of type {1}", table2.Key, table1.Key));

            return memberInfo[0].Name;
        }

        public ISession Session
        {
            get { return _session; }
        }

        public int SrmDocumentRevisionIndex { get; private set; }
        /// <summary>
        /// Add all of the information from the SrmDocument into the tables.
        /// </summary>
        public void AddSrmDocument(SrmDocument srmDocument)
        {
            SrmDocumentRevisionIndex = srmDocument.RevisionIndex;
            _dataSettings = srmDocument.Settings.DataSettings;
            DocInfo docInfo = new DocInfo(srmDocument);
            using (var transaction = _session.BeginTransaction())
            {
                SaveResults(_session, docInfo);

                // Progress reporting numbers
                int peptideCount = srmDocument.PeptideCount;
                int peptideTotal = 0;

                foreach (PeptideGroupDocNode nodeGroup in srmDocument.PeptideGroups)
                {
                    PeptideGroup peptideGroup = nodeGroup.PeptideGroup;
                    DbProtein dbProtein = new DbProtein
                    {
                        Name = nodeGroup.Name,
                        Description = nodeGroup.Description,
                        Sequence = peptideGroup.Sequence,
                        Note = nodeGroup.Note
                    };
                    AddAnnotations(docInfo, dbProtein, nodeGroup.Annotations);
                    _session.Save(dbProtein);
                    Dictionary<DbResultFile, DbProteinResult> proteinResults = new Dictionary<DbResultFile, DbProteinResult>();
                    if (srmDocument.Settings.HasResults)
                    {
                        foreach (var replicateFiles in docInfo.ReplicateResultFiles)
                        {
                            foreach (var replicateFile in replicateFiles.Select(fi => fi.ResultFile))
                            {
                                DbProteinResult proteinResult = new DbProteinResult
                                {
                                    ResultFile = replicateFile,
                                    Protein = dbProtein,
                                    FileName = replicateFile.FileName,
                                    SampleName = replicateFile.SampleName,
                                    ModifiedTime = replicateFile.ModifiedTime,
                                    AcquiredTime = replicateFile.AcquiredTime,
                                    ReplicateName = replicateFile.Replicate.Replicate,
                                    ReplicatePath = replicateFile.Replicate.ReplicatePath
                                };
                                _session.Save(proteinResult);
                                proteinResults.Add(replicateFile, proteinResult);
                            }
                        }
                    }
                    docInfo.ProteinResults.Add(dbProtein, proteinResults);
                    foreach (PeptideDocNode nodePeptide in nodeGroup.Children)
                    {
                        if (LongWaitBroker != null)
                        {
                            if (LongWaitBroker.IsCanceled)
                                return;
                            LongWaitBroker.ProgressValue = ++peptideTotal * PercentOfWait / peptideCount;
                        }

                        SavePeptide(_session, docInfo, dbProtein, nodePeptide);
                    }
                    _session.Flush();
                    _session.Clear();
                }
                transaction.Commit();
            }
        }

        private static void AddAnnotations(DocInfo docInfo, DbEntity dbEntity, Annotations annotations)
        {
            foreach (var entry in annotations.ListAnnotations())
            {
                dbEntity.Annotations[docInfo.GetAnnotationKey(entry.Key)] = entry.Value;
            }
        }

        private static void SaveResults(ISession session, DocInfo docInfo)
        {
            if (docInfo.MeasuredResults == null)
                return;

            foreach (ChromatogramSet chromatogramSet in docInfo.MeasuredResults.Chromatograms)
            {
                DbReplicate dbReplicate = new DbReplicate {Replicate = chromatogramSet.Name, ReplicatePath = "/"};
                session.Save(dbReplicate);

                var listResultFiles = new MappedList<int, ResultFileAndIndex>();
                docInfo.ReplicateResultFiles.Add(listResultFiles);

                foreach (var fileInfo in chromatogramSet.MSDataFileInfos)
                {
                    string filePath = fileInfo.FilePath;
                    DbResultFile dbResultFile = new DbResultFile
                                                    {
                                                        Replicate = dbReplicate,
                                                        FileName = SampleHelp.GetFileName(filePath),
                                                        SampleName = SampleHelp.GetFileSampleName(filePath),
                                                        ModifiedTime = fileInfo.FileWriteTime,
                                                        AcquiredTime = fileInfo.RunStartTime
                                                    };
                    session.Save(dbResultFile);
                    
                    listResultFiles.Add(new ResultFileAndIndex(dbResultFile, fileInfo.FileIndex));
                }
            }
            session.Flush();
            session.Clear();
        }

        /// <summary>
        /// Inserts rows for the peptide and all of its results and children.
        /// </summary>
        private static void SavePeptide(ISession session, DocInfo docInfo,
            DbProtein dbProtein, PeptideDocNode nodePeptide)
        {
            Peptide peptide = nodePeptide.Peptide;
            DbPeptide dbPeptide = new DbPeptide
            {
                Protein = dbProtein,
                Sequence = peptide.Sequence,
                BeginPos = peptide.Begin,
                // Convert from a non-inclusive end to an inclusive end
                EndPos = (peptide.End.HasValue ? peptide.End.Value - 1 : (int?) null),
                MissedCleavages = peptide.MissedCleavages,
                Note = nodePeptide.Note,
                AverageMeasuredRetentionTime = nodePeptide.AverageMeasuredRetentionTime,
            };
            double? scoreCalc = null;
            if (docInfo.PeptidePrediction.RetentionTime != null)
            {
                string modSeq = docInfo.Settings.GetModifiedSequence(nodePeptide);
                scoreCalc = docInfo.PeptidePrediction.RetentionTime.Calculator.ScoreSequence(modSeq);
                dbPeptide.RetentionTimeCalculatorScore = scoreCalc;
                if (scoreCalc.HasValue)
                {
                    double? rt = docInfo.PeptidePrediction.RetentionTime.GetRetentionTime(scoreCalc.Value);
                    dbPeptide.PredictedRetentionTime = rt;
                }
            }
            AddAnnotations(docInfo, dbPeptide, nodePeptide.Annotations);
            session.Save(dbPeptide);
            var peptideResults = new Dictionary<DbResultFile, DbPeptideResult>();
            docInfo.PeptideResults.Add(dbPeptide, peptideResults);
            if (nodePeptide.HasResults)
            {
                int iBest = nodePeptide.BestResult;
                for (int i = 0, len = nodePeptide.Results.Count; i < len; i++)
                {
                    var results = nodePeptide.Results[i];
                    if (results == null)
                        continue;

                    var resultFiles = docInfo.ReplicateResultFiles[i];

                    foreach (var chromInfo in results)
                    {
                        if (chromInfo == null)
                            continue;

                        var resultFile = resultFiles[chromInfo.FileIndex].ResultFile;
                        DbPeptideResult dbPeptideResult = new DbPeptideResult
                        {
                            Peptide = dbPeptide,
                            ResultFile = resultFile,
                            PeptidePeakFoundRatio = chromInfo.PeakCountRatio,
                            PeptideRetentionTime = chromInfo.RetentionTime,
                            BestReplicate = (i == iBest),
                            ProteinResult = docInfo.ProteinResults[dbProtein][resultFile],
                        };
                        if (scoreCalc.HasValue)
                        {
                            double? rt = docInfo.PeptidePrediction.RetentionTime.GetRetentionTime(scoreCalc.Value,
                                                                                                  chromInfo.FileId);
                            dbPeptideResult.PredictedResultRetentionTime = rt;
                        }
                        AddRatios(dbPeptideResult, docInfo, chromInfo);
                        session.Save(dbPeptideResult);
                        peptideResults.Add(resultFile, dbPeptideResult);
                    }
                }
            }
            session.Flush();
            session.Clear();
            foreach (TransitionGroupDocNode nodeGroup in nodePeptide.Children)
            {
                SavePrecursor(session, docInfo, dbPeptide, nodePeptide, nodeGroup);
            }
        }

        private static void AddRatios(DbPeptideResult peptideResult, DocInfo docInfo,
                PeptideChromInfo chromInfo)
        {
            var mods = docInfo.Settings.PeptideSettings.Modifications;
            var standardTypes = mods.InternalStandardTypes;
            var labelTypes = mods.GetModificationTypes().ToArray();
            bool firstRatio = true;
            foreach (var standardType in standardTypes)
            {
                foreach (var labelType in labelTypes)
                {
                    if (ReferenceEquals(standardType, labelType))
                        continue;
                    if (firstRatio)
                    {
                        peptideResult.RatioToStandard = GetPeptideRatio(chromInfo, labelType, standardType);
                        firstRatio = false;
                    }
                    // If there are more than two label types, add all the possible
                    // ratio combinations as custom columns
                    if (labelTypes.Length > 2)
                    {
                        string keyRatio = RatioPropertyAccessor.GetPeptideKey(labelType, standardType);
                        peptideResult.LabelRatios[keyRatio] =
                            GetPeptideRatio(chromInfo, labelType, standardType);
                    }
                }
            }
        }

        private static double? GetPeptideRatio(PeptideChromInfo chromInfo,
            IsotopeLabelType labelType, IsotopeLabelType standardType)
        {
            foreach (var labelRatio in chromInfo.LabelRatios)
            {
                if (ReferenceEquals(labelType, labelRatio.LabelType) &&
                        ReferenceEquals(standardType, labelRatio.StandardType))
                    return labelRatio.Ratio;
            }
            return null;
        }

        /// <summary>
        /// Inserts rows for the precursor and all of its results and children.
        /// </summary>
        private static void SavePrecursor(ISession session, DocInfo docInfo,
            DbPeptide dbPeptide, PeptideDocNode nodePeptide, TransitionGroupDocNode nodeGroup)
        {
            var predictTran = docInfo.Settings.TransitionSettings.Prediction;

            var calcPre = docInfo.Settings.GetPrecursorCalc(nodeGroup.TransitionGroup.LabelType, nodePeptide.ExplicitMods);
            string seq = nodeGroup.TransitionGroup.Peptide.Sequence;
            string seqModified = calcPre.GetModifiedSequence(seq, true);

            TransitionGroup tranGroup = nodeGroup.TransitionGroup;
            IsotopeLabelType labelType = tranGroup.LabelType;
            DbPrecursor dbPrecursor = new DbPrecursor
                                        {
                                            Peptide = dbPeptide,
                                            ModifiedSequence = seqModified,
                                            Charge = tranGroup.PrecursorCharge,
                                            IsotopeLabelType = tranGroup.LabelType,
                                            NeutralMass = SequenceMassCalc.PersistentNeutral(SequenceMassCalc.GetMH(nodeGroup.PrecursorMz, tranGroup.PrecursorCharge)),
                                            Mz = SequenceMassCalc.PersistentMZ(nodeGroup.PrecursorMz),
                                            Note = nodeGroup.Note,
                                            IsDecoy = nodeGroup.IsDecoy,
                                            DecoyMzShift = tranGroup.DecoyMassShift
                                        };
            AddAnnotations(docInfo, dbPrecursor, nodeGroup.Annotations);
            double regressionMz = docInfo.Settings.GetRegressionMz(nodePeptide, nodeGroup);
            dbPrecursor.CollisionEnergy = predictTran.CollisionEnergy.GetCollisionEnergy(
                tranGroup.PrecursorCharge, regressionMz);
            if (predictTran.DeclusteringPotential != null)
            {
                dbPrecursor.DeclusteringPotential = predictTran.DeclusteringPotential.GetDeclustringPotential(
                    regressionMz);
            }

            if (nodeGroup.HasLibInfo)
            {
                var libInfo = nodeGroup.LibInfo;
                dbPrecursor.LibraryName = libInfo.LibraryName;
                int iValue = 0;
                if (libInfo is NistSpectrumHeaderInfo)
                    dbPrecursor.LibraryType = "NIST";
                else if (libInfo is XHunterSpectrumHeaderInfo)
                    dbPrecursor.LibraryType = "GPM";
                else if (libInfo is BiblioSpecSpectrumHeaderInfo)
                    dbPrecursor.LibraryType = "BiblioSpec";
                foreach (var pair in libInfo.RankValues)
                {
                    switch (iValue)
                    {
                        case 0:
                            dbPrecursor.LibraryScore1 = libInfo.GetRankValue(pair.Key);
                            break;
                        case 1:
                            dbPrecursor.LibraryScore2 = libInfo.GetRankValue(pair.Key);
                            break;
                        case 2:
                            dbPrecursor.LibraryScore3 = libInfo.GetRankValue(pair.Key);
                            break;
                    }
                    iValue++;
                }
            }
            
            session.Save(dbPrecursor);
            var precursorResults = new Dictionary<ResultKey, DbPrecursorResult>();
            docInfo.PrecursorResults.Add(dbPrecursor, precursorResults);
            var peptideResults = docInfo.PeptideResults[dbPeptide];
            DbPrecursorResultSummary precursorResultSummary = null;

            if (nodeGroup.HasResults)
            {
                // Values for summary statistics
                var precursorSummaryValues = new PrecursorSummaryValues();

                for (int i = 0; i < nodeGroup.Results.Count; i++)
                {
                    var results = nodeGroup.Results[i];
                    if (results == null)
                        continue;

                    var optFunction = docInfo.MeasuredResults.Chromatograms[i].OptimizationFunction;
                    var replicateSummary = docInfo.ReplicateSummaries[i];
                    var resultFiles = docInfo.ReplicateResultFiles[i];

                    foreach (var chromInfo in results)
                    {
                        if (chromInfo == null)
                            continue;

                        var resultFile = resultFiles[chromInfo.FileIndex].ResultFile;
                        DbPrecursorResult precursorResult = new DbPrecursorResult
                        {
                            Precursor = dbPrecursor,
                            ResultFile = resultFile,
                            PrecursorPeakFoundRatio = chromInfo.PeakCountRatio,
                            BestRetentionTime = chromInfo.RetentionTime,
                            MinStartTime = chromInfo.StartRetentionTime,
                            MaxEndTime = chromInfo.EndRetentionTime,
                            MaxFwhm = chromInfo.Fwhm,
                            TotalArea = chromInfo.Area,
                            TotalBackground = chromInfo.BackgroundArea,
                            TotalAreaRatio = chromInfo.Ratios[0],
                            // StdevAreaRatio = chromInfo.RatioStdevs[0],
                            CountTruncated = chromInfo.Truncated,
                            Identified = chromInfo.Identified,
                            LibraryDotProduct = chromInfo.LibraryDotProduct,
                            IsotopeDotProduct = chromInfo.IsotopeDotProduct,
                            // TotalSignalToNoise = SignalToNoise(chromInfo.Area, chromInfo.BackgroundArea),
                            Note = chromInfo.Annotations.Note,
                            UserSetTotal = chromInfo.UserSet,
                            PeptideResult = peptideResults[resultFile],
                            // Set the optimization step no matter what, so that replicates without
                            // optimization data will join with those with it.
                            OptStep = chromInfo.OptimizationStep,
                        };
                        var standardTypes = docInfo.Settings.PeptideSettings.Modifications.InternalStandardTypes;
                        if (standardTypes.Count > 1)
                        {
                            for (int j = 0; j < standardTypes.Count; j++)
                            {
                                string columnName = RatioPropertyAccessor.GetPrecursorKey(standardTypes[j]);
                                precursorResult.LabelRatios[columnName] = chromInfo.Ratios[j];
                            }
                        }
                        // Area values get normalized to the total for the precursors of the same isotope
                        // label type, since this allows use of CVs on spiked in heavy labeled peptides.
                        double sumTotalArea;
                        if (chromInfo.Area.HasValue && replicateSummary.DictTotalAreas.TryGetValue(labelType, out sumTotalArea) && sumTotalArea != 0)
                            precursorResult.TotalAreaNormalized = chromInfo.Area/sumTotalArea;
                        AddAnnotations(docInfo, precursorResult, chromInfo.Annotations);
                        // Set the optimization step no matter what, so that replicates without
                        // optimization data will join with those with it.
                        precursorResult.OptStep = chromInfo.OptimizationStep;
                        if (optFunction != null)
                        {
                            var ceRegression = optFunction as CollisionEnergyRegression;
                            if (ceRegression != null)
                            {
                                precursorResult.OptCollisionEnergy = ceRegression.GetCollisionEnergy(
                                        dbPrecursor.Charge, regressionMz, chromInfo.OptimizationStep);
                            }
                            var dpRegression = optFunction as DeclusteringPotentialRegression;
                            if (dpRegression != null)
                            {
                                precursorResult.OptDeclusteringPotential = dpRegression.GetDeclustringPotential(
                                        regressionMz, chromInfo.OptimizationStep);
                            }
                        }
                        session.Save(precursorResult);
                        precursorResults.Add(new ResultKey(resultFile, chromInfo.OptimizationStep), precursorResult);
                        if (chromInfo.OptimizationStep == 0)
                            precursorSummaryValues.Add(precursorResult);
                    }
                }
                
                precursorResultSummary = new DbPrecursorResultSummary
                {
                    Precursor = dbPrecursor
                };

                precursorSummaryValues.CalculateStatistics(precursorResultSummary);
                session.Save(precursorResultSummary);
            }
            session.Flush();
            session.Clear();
            foreach (TransitionDocNode nodeTran in nodeGroup.Children)
            {
                SaveTransition(session, docInfo, dbPrecursor, precursorResultSummary, nodeTran);
            }
        }

        /// <summary>
        /// Inserts rows for the transition and all of results.
        /// </summary>
        private static void SaveTransition(ISession session,
                                           DocInfo docInfo,
                                           DbPrecursor dbPrecursor,
                                           DbPrecursorResultSummary precursorResultSummary,
                                           TransitionDocNode nodeTran)
        {
            Transition transition = nodeTran.Transition;
            IsotopeLabelType labelType = transition.Group.LabelType;
            string fragmentIon = transition.FragmentIonName;
            if (transition.IonType == IonType.precursor)
                fragmentIon += Transition.GetMassIndexText(transition.MassIndex);
            DbTransition dbTransition = new DbTransition
                                          {
                                              Precursor = dbPrecursor,
                                              ProductCharge = transition.Charge,
                                              ProductNeutralMass = SequenceMassCalc.PersistentNeutral(SequenceMassCalc.GetMH(nodeTran.Mz, transition.Charge)),
                                              ProductMz = SequenceMassCalc.PersistentMZ(nodeTran.Mz),
                                              FragmentIon = fragmentIon,
                                              FragmentIonType = transition.IonType.ToString(),
                                              FragmentIonOrdinal = transition.Ordinal,
                                              CleavageAa = transition.AA.ToString(CultureInfo.InvariantCulture),
                                              LossNeutralMass = nodeTran.LostMass,
                                              Note = nodeTran.Note,
                                              IsotopeDistIndex = transition.MassIndex,
                                              IsDecoy = nodeTran.IsDecoy,
                                              ProductDecoyMzShift = transition.DecoyMassShift
                                          };

            if (nodeTran.HasLoss)
            {
                dbTransition.Losses = string.Join(", ", nodeTran.Losses.ToStrings());
            }
            if (nodeTran.HasLibInfo)
            {
                dbTransition.LibraryIntensity = nodeTran.LibInfo.Intensity;
                dbTransition.LibraryRank = nodeTran.LibInfo.Rank;
            }
            if (nodeTran.HasDistInfo)
            {
                dbTransition.IsotopeDistRank = nodeTran.IsotopeDistInfo.Rank;
                dbTransition.IsotopeDistProportion = nodeTran.IsotopeDistInfo.Proportion;
            }
            var fullScan = docInfo.Settings.TransitionSettings.FullScan;
            if (fullScan.IsEnabledMs && transition.IonType == IonType.precursor)
            {
                dbTransition.FullScanFilterWidth = SequenceMassCalc.PersistentMZ(
                    fullScan.GetPrecursorFilterWindow(nodeTran.Mz));
            }
            else if (fullScan.IsEnabledMsMs &&
                    (transition.IonType != IonType.precursor || transition.MassIndex == 0))
            {
                dbTransition.FullScanFilterWidth = SequenceMassCalc.PersistentMZ(
                    fullScan.GetProductFilterWindow(nodeTran.Mz));
            }
            AddAnnotations(docInfo, dbTransition, nodeTran.Annotations);
            session.Save(dbTransition);
            var precursorResults = docInfo.PrecursorResults[dbPrecursor];
            if (nodeTran.HasResults)
            {
                // Values for summary statistics
                var transitionSummaryValues = new TransitionSummaryValues();

                for (int i = 0; i < nodeTran.Results.Count; i++)
                {
                    var results = nodeTran.Results[i];

                    if (results == null)
                        continue;

                    var replicateSummary = docInfo.ReplicateSummaries[i];
                    var resultFiles = docInfo.ReplicateResultFiles[i];

                    foreach (var chromInfo in results)
                    {
                        if (chromInfo == null)
                            continue;

                        var resultFile = resultFiles[chromInfo.FileIndex].ResultFile;
                        DbTransitionResult transitionResult = new DbTransitionResult
                        {
                            Transition = dbTransition,
                            ResultFile = resultFile,
                            OptStep = chromInfo.OptimizationStep,
                            AreaRatio = chromInfo.Ratios[0],
                            Note = chromInfo.Annotations.Note,
                            UserSetPeak = chromInfo.UserSet,
                            PrecursorResult = precursorResults[new ResultKey(resultFile,chromInfo.OptimizationStep)],
                        };
                        var standardTypes = docInfo.Settings.PeptideSettings.Modifications.InternalStandardTypes;
                        if (standardTypes.Count > 1)
                        {
                            for (int j = 0; j < standardTypes.Count; j++)
                            {
                                string columnName = RatioPropertyAccessor.GetTransitionKey(standardTypes[j]);
                                transitionResult.LabelRatios[columnName] = chromInfo.Ratios[j];
                            }
                        }
                        AddAnnotations(docInfo, transitionResult, chromInfo.Annotations);
                        if (!chromInfo.IsEmpty)
                        {
                            transitionResult.RetentionTime = chromInfo.RetentionTime;
                            transitionResult.StartTime = chromInfo.StartRetentionTime;
                            transitionResult.EndTime = chromInfo.EndRetentionTime;
                            transitionResult.Area = chromInfo.Area;
                            // Area values get normalized to the total for the precursors of the same isotope
                            // label type, since this allows use of CVs on spiked in heavy labeled peptides.
                            double sumTotalArea;
                            if (replicateSummary.DictTotalAreas.TryGetValue(labelType, out sumTotalArea) && sumTotalArea != 0)
                                transitionResult.AreaNormalized = chromInfo.Area/sumTotalArea;
                            transitionResult.Background = chromInfo.BackgroundArea;
                            // transitionResult.SignalToNoise = SignalToNoise(chromInfo.Area, chromInfo.BackgroundArea);
                            transitionResult.Height = chromInfo.Height;
                            transitionResult.Fwhm = chromInfo.Fwhm;
                            transitionResult.FwhmDegenerate = chromInfo.IsFwhmDegenerate;
                            transitionResult.Truncated = chromInfo.IsTruncated;
                            transitionResult.PeakRank = chromInfo.Rank;
                        }
                        session.Save(transitionResult);
                        if (chromInfo.OptimizationStep == 0)
                            transitionSummaryValues.Add(transitionResult);
                    }
                }

                var transitionResultSummary = new DbTransitionResultSummary
                {
                    Transition = dbTransition,
                    PrecursorResultSummary = precursorResultSummary
                };

                transitionSummaryValues.CalculateStatistics(transitionResultSummary);
                session.Save(transitionResultSummary);
            }
            session.Flush();
            session.Clear();
        }

//        private static double SignalToNoise(float area, float background)
//        {
//            // TODO: Figure out the real equation for this
//            return 20 * Math.Log10(background != 0 ? area / background : 1000000);
//        }

        /// <summary>
        /// Holds information about the entire document that is passed around while
        /// we are populating the database.
        /// </summary>
        private class DocInfo
        {
            public DocInfo(SrmDocument srmDocument)
            {
                Settings = srmDocument.Settings;

                // Create replicate summaries
                ReplicateSummaries = new List<ReplicateSummaryValues>();
                if (Settings.HasResults)
                {
                    for (int i = 0; i < MeasuredResults.Chromatograms.Count; i++)
                    {
                        var dictTotalAreas = new Dictionary<IsotopeLabelType, double>();
                        foreach (var nodeGroup in srmDocument.TransitionGroups)
                        {
                            // It should not be possible to not have results when
                            // the settings have results, but just to be safe
                            if (!nodeGroup.HasResults || nodeGroup.Results[i] == null)
                                continue;

                            var labelType = nodeGroup.TransitionGroup.LabelType;

                            foreach (var chromInfo in nodeGroup.Results[i])
                            {
                                if (chromInfo.OptimizationStep == 0)
                                {
                                    double sumTotalArea;
                                    if (!dictTotalAreas.TryGetValue(labelType, out sumTotalArea))
                                        sumTotalArea = 0;
                                    dictTotalAreas[labelType] = sumTotalArea + chromInfo.Area ?? 0;                                    
                                }
                            }
                        }
                        ReplicateSummaries.Add(new ReplicateSummaryValues(dictTotalAreas));
                    }
                }
                ReplicateResultFiles = new List<MappedList<int, ResultFileAndIndex>>();
                ProteinResults = new Dictionary<DbProtein, Dictionary<DbResultFile, DbProteinResult>>();
                PeptideResults = new Dictionary<DbPeptide, Dictionary<DbResultFile, DbPeptideResult>>();
                PrecursorResults = new Dictionary<DbPrecursor, Dictionary<ResultKey, DbPrecursorResult>>();
                AnnotationKeys = new Dictionary<string, string>();
            }

            public SrmSettings Settings { get; private set; }
            public PeptidePrediction PeptidePrediction { get { return Settings.PeptideSettings.Prediction; } }
            public MeasuredResults MeasuredResults { get { return Settings.MeasuredResults; } }
            public List<ReplicateSummaryValues> ReplicateSummaries { get; private set; }
            public List<MappedList<int, ResultFileAndIndex>> ReplicateResultFiles { get; private set; }
            public Dictionary<DbProtein, Dictionary<DbResultFile, DbProteinResult>> ProteinResults { get; private set; }
            public Dictionary<DbPeptide, Dictionary<DbResultFile, DbPeptideResult>> PeptideResults { get; private set; }
            public Dictionary<DbPrecursor, Dictionary<ResultKey, DbPrecursorResult>> PrecursorResults { get; private set; }

            private Dictionary<string, string> AnnotationKeys { get; set; }

            public string GetAnnotationKey(string annotationName)
            {
                string key;
                if (!AnnotationKeys.TryGetValue(annotationName, out key))
                {
                    key = AnnotationDef.GetKey(annotationName);
                    AnnotationKeys[annotationName] = key;
                }
                return key;
            }
        }

        class ResultFileAndIndex : IKeyContainer<int>
        {
            private readonly int _globalIndex;

            public ResultFileAndIndex(DbResultFile resultFile, int globalIndex)
            {
                ResultFile = resultFile;
                _globalIndex = globalIndex;
            }

            public DbResultFile ResultFile { get; private set; }

            public int GetKey()
            {
                return _globalIndex;
            }
        }

        class ReplicateSummaryValues
        {
            public ReplicateSummaryValues(Dictionary<IsotopeLabelType, double> dictTotalAreas)
            {
                DictTotalAreas = dictTotalAreas;
            }

            public Dictionary<IsotopeLabelType, double> DictTotalAreas { get; private set; }
        }

        class PrecursorSummaryValues
        {
            private readonly List<DbPrecursorResult> _results = new List<DbPrecursorResult>();

            public void Add(DbPrecursorResult result)
            {
                _results.Add(result);
            }

// ReSharper disable PossibleInvalidOperationException
            private Statistics BestRetentionTimeStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.BestRetentionTime.HasValue
                                          select result.BestRetentionTime.Value);
                }
            }

            private Statistics MaxFwhmStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.MaxFwhm.HasValue
                                          select result.MaxFwhm.Value);
                }
            }

            private Statistics TotalAreaStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.TotalArea.HasValue
                                          select result.TotalArea.Value);
                }
            }

            private Statistics TotalAreaRatioStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.TotalAreaRatio.HasValue
                                          select result.TotalAreaRatio.Value);
                }
            }

            private Statistics TotalAreaNormalizedStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.TotalAreaNormalized.HasValue
                                          select result.TotalAreaNormalized.Value);
                }
            }
// ReSharper restore PossibleInvalidOperationException

            public void CalculateStatistics(DbPrecursorResultSummary precursorResultSummary)
            {
                precursorResultSummary.ReplicatePath = "/";
                var bestRetentionTimeStats = BestRetentionTimeStats;
                if (bestRetentionTimeStats.Length > 0)
                {
                    double minBestRetentionTime = bestRetentionTimeStats.Min();
                    double maxBestRetentionTime = bestRetentionTimeStats.Max();

                    precursorResultSummary.MinBestRetentionTime = minBestRetentionTime;
                    precursorResultSummary.MaxBestRetentionTime = maxBestRetentionTime;
                    precursorResultSummary.RangeBestRetentionTime = maxBestRetentionTime - minBestRetentionTime;
                }
                CalcSummary(bestRetentionTimeStats, (mean, stdev, cv) =>
                {
                    precursorResultSummary.MeanBestRetentionTime = mean;
                    precursorResultSummary.StdevBestRetentionTime = stdev;
                    precursorResultSummary.CvBestRetentionTime = cv;
                });
                CalcSummary(MaxFwhmStats, (mean, stdev, cv) =>
                {
                    precursorResultSummary.MeanMaxFwhm = mean;
                    precursorResultSummary.StdevMaxFwhm = stdev;
                    precursorResultSummary.CvMaxFwhm = cv;
                });
                CalcSummary(TotalAreaStats, (mean, stdev, cv) =>
                {
                    precursorResultSummary.MeanTotalArea = mean;
                    precursorResultSummary.StdevTotalArea = stdev;
                    precursorResultSummary.CvTotalArea = cv;
                });
                CalcSummary(TotalAreaRatioStats, (mean, stdev, cv) =>
                {
                    precursorResultSummary.MeanTotalAreaRatio = mean;
                    precursorResultSummary.StdevTotalAreaRatio = stdev;
                    precursorResultSummary.CvTotalAreaRatio = cv;
                });
                CalcSummary(TotalAreaNormalizedStats, (mean, stdev, cv) =>
                {
                    precursorResultSummary.MeanTotalAreaNormalized = mean;
                    precursorResultSummary.StdevTotalAreaNormalized = stdev;
                    precursorResultSummary.CvTotalAreaNormalized = cv;
                });
            }
        }

        class TransitionSummaryValues
        {
            private readonly List<DbTransitionResult> _results = new List<DbTransitionResult>();

            public void Add(DbTransitionResult result)
            {
                _results.Add(result);
            }

// ReSharper disable PossibleInvalidOperationException
            private Statistics RetentionTimeStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.RetentionTime.HasValue
                                          select result.RetentionTime.Value);
                }
            }

            private Statistics FwhmStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.Fwhm.HasValue
                                          select result.Fwhm.Value);
                }
            }

            private Statistics AreaStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.Area.HasValue
                                          select result.Area.Value);
                }
            }

            private Statistics AreaRatioStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.AreaRatio.HasValue
                                          select result.AreaRatio.Value);
                }
            }

            private Statistics AreaNormalizedStats
            {
                get
                {
                    return new Statistics(from result in _results
                                          where result.AreaNormalized.HasValue
                                          select result.AreaNormalized.Value);
                }
            }
// ReSharper restore PossibleInvalidOperationException

            public void CalculateStatistics(DbTransitionResultSummary transitionResultSummary)
            {
                transitionResultSummary.ReplicatePath = "/";
                var retentionTimeStats = RetentionTimeStats;
                if (retentionTimeStats.Length > 0)
                {
                    double minRetentionTime = retentionTimeStats.Min();
                    double maxRetentionTime = retentionTimeStats.Max();

                    transitionResultSummary.MinRetentionTime = minRetentionTime;
                    transitionResultSummary.MaxRetentionTime = maxRetentionTime;
                    transitionResultSummary.RangeRetentionTime = maxRetentionTime - minRetentionTime;
                }
                CalcSummary(retentionTimeStats, (mean, stdev, cv) =>
                {
                    transitionResultSummary.MeanRetentionTime = mean;
                    transitionResultSummary.StdevRetentionTime = stdev;
                    transitionResultSummary.CvRetentionTime = cv;
                });
                CalcSummary(FwhmStats, (mean, stdev, cv) =>
                {
                    transitionResultSummary.MeanFwhm = mean;
                    transitionResultSummary.StdevFwhm = stdev;
                    transitionResultSummary.CvFwhm = cv;
                });
                CalcSummary(AreaStats, (mean, stdev, cv) =>
                {
                    transitionResultSummary.MeanArea = mean;
                    transitionResultSummary.StdevArea = stdev;
                    transitionResultSummary.CvArea = cv;
                });
                CalcSummary(AreaRatioStats, (mean, stdev, cv) =>
                {
                    transitionResultSummary.MeanAreaRatio = mean;
                    transitionResultSummary.StdevAreaRatio = stdev;
                    transitionResultSummary.CvAreaRatio = cv;
                });
                CalcSummary(AreaNormalizedStats, (mean, stdev, cv) =>
                {
                    transitionResultSummary.MeanAreaNormalized = mean;
                    transitionResultSummary.StdevAreaNormalized = stdev;
                    transitionResultSummary.CvAreaNormalized = cv;
                });
            }
        }

        public static void CalcSummary(Statistics stats, Action<double?, double?, double?> setStats)
        {
            double? mean = null, stdev = null, cv = null;
            if (stats.Length > 0)
            {
                mean = stats.Mean();
                if (stats.Length > 1)
                {
                    stdev = stats.StdDev();
                    cv = stdev / mean;
                }
            }
            setStats(mean, stdev, cv);
        }
    }

    struct ResultKey
    {
// ReSharper disable InconsistentNaming
        public readonly DbResultFile ResultFile;
        public readonly int OptimizationStep;
// ReSharper restore InconsistentNaming
        public ResultKey(DbResultFile resultFile, int optimizationStep)
        {
            ResultFile = resultFile;
            OptimizationStep = optimizationStep;
        }
    }
}
