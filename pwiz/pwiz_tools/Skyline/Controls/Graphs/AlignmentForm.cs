﻿/*
 * Original author: Nick Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2012 University of Washington - Seattle, WA
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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;
using pwiz.Skyline.EditUI;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Lib;
using pwiz.Skyline.Model.RetentionTimes;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Controls.Graphs
{
    public partial class AlignmentForm : Form
    {
        private readonly BindingList<DataRow> _dataRows = new BindingList<DataRow>()
                                                              {
                                                                  AllowEdit = false,
                                                                  AllowNew = false,
                                                                  AllowRemove = false,
                                                              };
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public AlignmentForm(SkylineWindow skylineWindow)
        {
            InitializeComponent();
            SkylineWindow = skylineWindow;
            Icon = Resources.Skyline;
            bindingSource.DataSource = _dataRows;
            colSlope.CellTemplate.Style.Format = "0.0000";
            colIntercept.CellTemplate.Style.Format = "0.0000";
            colCorrelationCoefficient.CellTemplate.Style.Format = "0.0000";
            colUnrefinedSlope.CellTemplate.Style.Format = "0.0000";
            colUnrefinedIntercept.CellTemplate.Style.Format = "0.0000";
            colUnrefinedCorrelationCoefficient.CellTemplate.Style.Format = "0.0000";
        }

        public SkylineWindow SkylineWindow { get; private set; }
        public SrmDocument Document
        {
            get { return SkylineWindow.DocumentUI; }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (SkylineWindow != null)
            {
                SkylineWindow.DocumentUIChangedEvent += SkylineWindowOnDocumentUIChangedEvent;
            }
            UpdateAll();
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (SkylineWindow != null)
            {
                SkylineWindow.DocumentUIChangedEvent -= SkylineWindowOnDocumentUIChangedEvent;
            }
            _cancellationTokenSource.Cancel();
            base.OnHandleDestroyed(e);
        }

        private void SkylineWindowOnDocumentUIChangedEvent(object sender, DocumentChangedEventArgs documentChangedEventArgs)
        {
            UpdateAll();
        }

        public void UpdateAll()
        {
            UpdateCombo();
        }

        public void UpdateGraph()
        {
            zedGraphControl.GraphPane.CurveList.Clear();
            zedGraphControl.GraphPane.GraphObjList.Clear();
            if (!(bindingSource.Current is DataRow))
            {
                return;
            }
            var currentRow = (DataRow) bindingSource.Current;
            var alignedFile = currentRow.AlignedRetentionTimes;
            if (alignedFile == null)
            {
                zedGraphControl.GraphPane.Title.Text = "Waiting for retention time alignment";
                return;
            }
            var points = new PointPairList();
            var outliers = new PointPairList();
            var peptideTimes = alignedFile.Regression.PeptideTimes;
            for (int i = 0; i < peptideTimes.Count; i++)
            {
                var peptideTime = peptideTimes[i];
                var point = new PointPair(alignedFile.OriginalTimes[peptideTime.PeptideSequence],
                                            peptideTime.RetentionTime,
                                            peptideTime.PeptideSequence);
                if (alignedFile.OutlierIndexes.Contains(i))
                {
                    outliers.Add(point);
                }
                else
                {
                    points.Add(point);
                }
            }

            var goodPointsLineItem = new LineItem("Peptides", points, Color.Black, SymbolType.Diamond);
            goodPointsLineItem.Symbol.Size = 8f;
            goodPointsLineItem.Line.IsVisible = false;
            goodPointsLineItem.Symbol.Border.IsVisible = false;
            goodPointsLineItem.Symbol.Fill = new Fill(RTLinearRegressionGraphPane.COLOR_REFINED);
            
            if (outliers.Count > 0)
            {
                var outlierLineItem = zedGraphControl.GraphPane.AddCurve("Outliers", outliers, Color.Black,
                                                                        SymbolType.Diamond);
                outlierLineItem.Symbol.Size = 8f;
                outlierLineItem.Line.IsVisible = false;
                outlierLineItem.Symbol.Border.IsVisible = false;
                outlierLineItem.Symbol.Fill = new Fill(RTLinearRegressionGraphPane.COLOR_OUTLIERS);
                goodPointsLineItem.Label.Text = "Peptides Refined";
            }
            zedGraphControl.GraphPane.CurveList.Add(goodPointsLineItem);
            if (points.Count > 0)
            {
                double xMin = points.Select(p => p.X).Min();
                double xMax = points.Select(p => p.X).Max();
                var regression = alignedFile.RegressionRefined ?? alignedFile.Regression;
                var regressionLine = zedGraphControl.GraphPane
                    .AddCurve("Regression line", new[] { xMin, xMax },
                        new[] { regression.Conversion.GetY(xMin), regression.Conversion.GetY(xMax) },
                        Color.Black);
                regressionLine.Symbol.IsVisible = false;
            }
            zedGraphControl.GraphPane.Title.Text = string.Format("Alignment of {0} to {1}",
                currentRow.DataFile,
                currentRow.Target.Name);
            zedGraphControl.GraphPane.XAxis.Title.Text 
                = string.Format("Time from {0}", currentRow.DataFile);
            zedGraphControl.GraphPane.YAxis.Title.Text = "Aligned time";
            zedGraphControl.GraphPane.AxisChange();
            zedGraphControl.Invalidate();
        }

        private void AlignDataRow(int index, CancellationToken cancellationToken)
        {
            var dataRow = _dataRows[index];
            if (dataRow.TargetTimes == null || dataRow.SourceTimes == null)
            {
                return;
            }
            Task.Factory.StartNew(
                () => {
                    try
                    {
                        return AlignedRetentionTimes.AlignLibraryRetentionTimes(
                            dataRow.TargetTimes, dataRow.SourceTimes,
                            DocumentRetentionTimes.REFINEMENT_THRESHHOLD,
                            () => cancellationToken.IsCancellationRequested);
                    }
                    catch (OperationCanceledException operationCanceledException)
                    {
                        throw new OperationCanceledException(operationCanceledException.Message, operationCanceledException, cancellationToken);
                    }
                }, cancellationToken)
                .ContinueWith(alignedTimesTask => UpdateDataRow(index, alignedTimesTask), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateDataRow(int iRow, Task<AlignedRetentionTimes> alignedTimesTask)
        {
            if (alignedTimesTask.IsCanceled)
            {
                return;
            }
            var dataRow = _dataRows[iRow];
            dataRow.AlignedRetentionTimes = alignedTimesTask.Result;
            _dataRows[iRow] = dataRow;
        }
        
        public void UpdateRows()
        {
            var newRows = GetRows();
            if (newRows.SequenceEqual(_dataRows))
            {
                return;
            }
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _dataRows.RaiseListChangedEvents = false;
            _dataRows.Clear();
            foreach (var row in newRows)
            {
                _dataRows.Add(row);
            }
            bool allSameLibrary = true;
            if (newRows.Count > 0)
            {
                var firstLibrary = newRows[0].Library;
                allSameLibrary = newRows.Skip(1).All(row => Equals(row.Library, firstLibrary));
            }
            colLibrary.Visible = !allSameLibrary;
            _dataRows.RaiseListChangedEvents = true;
            _dataRows.ResetBindings();
            for (int i = 0; i < _dataRows.Count; i++ )
            {
                AlignDataRow(i, _cancellationTokenSource.Token);
            }
            UpdateGraph();
        }
        private void UpdateCombo()
        {
            var documentRetentionTimes = Document.Settings.DocumentRetentionTimes;
            var newItems = documentRetentionTimes.RetentionTimeSources.Values.Select(retentionTimeSource=>new DataFileKey(retentionTimeSource)).ToArray();
            if (newItems.SequenceEqual(comboAlignAgainst.Items.Cast<DataFileKey>()))
            {
                return;
            }
            var selectedIndex = comboAlignAgainst.SelectedIndex;
            comboAlignAgainst.Items.Clear();
            comboAlignAgainst.Items.AddRange(newItems.Cast<object>().ToArray());
            ComboHelper.AutoSizeDropDown(comboAlignAgainst);
            if (comboAlignAgainst.Items.Count > 0)
            {
                if (selectedIndex < 0)
                {
                    if (SkylineWindow.SelectedResultsIndex >= 0)
                    {
                        var chromatogramSet =
                            Document.Settings.MeasuredResults.Chromatograms[SkylineWindow.SelectedResultsIndex];
                        foreach (var msDataFileInfo in chromatogramSet.MSDataFileInfos)
                        {
                            var retentionTimeSource = documentRetentionTimes.RetentionTimeSources.Find(msDataFileInfo);
                            if (retentionTimeSource == null)
                            {
                                continue;
                            }
                            selectedIndex =
                                newItems.IndexOf(
                                    dataFileKey => Equals(retentionTimeSource, dataFileKey.RetentionTimeSource));
                            break;
                        }
                    }
                }
                comboAlignAgainst.SelectedIndex = Math.Min(comboAlignAgainst.Items.Count - 1, 
                    Math.Max(0, selectedIndex));
            }
            UpdateRows();
        }

        private IList<DataRow> GetRows()
        {
            var targetKey = comboAlignAgainst.SelectedItem as DataFileKey?;
            if (!targetKey.HasValue)
            {
                return new DataRow[0];
            }
            var documentRetentionTimes = Document.Settings.DocumentRetentionTimes;
            var dataRows = new List<DataRow>();
            foreach (var retentionTimeSource in documentRetentionTimes.RetentionTimeSources.Values)
            {
                if (targetKey.Value.RetentionTimeSource.Name == retentionTimeSource.Name)
                {
                    continue;
                }
                dataRows.Add(new DataRow(Document.Settings, targetKey.Value.RetentionTimeSource, retentionTimeSource));
            }
            return dataRows;
        }

        internal struct DataRow
        {
            public DataRow(SrmSettings settings, RetentionTimeSource target, RetentionTimeSource timesToAlign) : this()
            {
                DocumentRetentionTimes = settings.DocumentRetentionTimes;
                Target = target;
                Source = timesToAlign;
                var fileAlignment = DocumentRetentionTimes.FileAlignments.Find(target.Name);
                if (fileAlignment != null)
                {
                    Alignment = fileAlignment.RetentionTimeAlignments.Find(Source.Name);
                }
                TargetTimes = settings.GetRetentionTimes(target.Name).GetFirstRetentionTimes();
                SourceTimes = settings.GetRetentionTimes(timesToAlign.Name).GetFirstRetentionTimes();
            }

            internal DocumentRetentionTimes DocumentRetentionTimes { get; private set; }
            internal RetentionTimeSource Target { get; private set; }
            internal RetentionTimeSource Source { get; private set; }
            internal RetentionTimeAlignment Alignment { get; private set; }
            internal IDictionary<string, double> TargetTimes { get; private set; }
            internal IDictionary<string, double> SourceTimes { get; private set; }
            public AlignedRetentionTimes AlignedRetentionTimes { get; set; }

            public String DataFile { get { return Source.Name; } }
            public string Library { get { return Source.Library; } }
            public RegressionLine RegressionLine
            {
                get 
                { 
                    if (AlignedRetentionTimes != null)
                    {
                        var regression = AlignedRetentionTimes.RegressionRefined ?? AlignedRetentionTimes.Regression;
                        if (regression != null)
                        {
                            return new RegressionLine(regression.Conversion.Slope, regression.Conversion.Intercept);
                        }
                    }
                    if (Alignment != null)
                    {
                        return Alignment.RegressionLine;
                    }
                    return null;
                }
            }
            public double? Slope
            {
                get
                {
                    var regressionLine = RegressionLine;
                    if (regressionLine != null)
                    {
                        return regressionLine.Slope;
                    }
                    return null;
                }
            }
            public double? Intercept
            {
                get
                {
                    var regressionLine = RegressionLine;
                    if (regressionLine != null)
                    {
                        return regressionLine.Intercept;
                    }
                    return null;
                }
            }
            public double? CorrelationCoefficient
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.RegressionRefinedStatistics.R;
                }
            }
            public int? OutlierCount
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.OutlierIndexes.Count;
                }
            }
            public double? UnrefinedSlope
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.Regression.Conversion.Slope;
                }
            }
            public double? UnrefinedIntercept
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.Regression.Conversion.Intercept;
                }
            }
            public double? UnrefinedCorrelationCoefficient
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.RegressionStatistics.R;
                }
            }
            public int? PointCount
            {
                get
                {
                    if (AlignedRetentionTimes == null)
                    {
                        return null;
                    }
                    return AlignedRetentionTimes.RegressionStatistics.Peptides.Count;
                }
            }
        }

        internal struct DataFileKey
        {
            public DataFileKey(RetentionTimeSource retentionTimeSource) : this()
            {
                RetentionTimeSource = retentionTimeSource;
            }

            public RetentionTimeSource RetentionTimeSource { get; private set; }
            public override string ToString()
            {
                return RetentionTimeSource.Name;
            }
        }

        private void comboAlignAgainst_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRows();
        }

        private void bindingSource_CurrentItemChanged(object sender, EventArgs e)
        {
            UpdateGraph();
        }

        #region Public members for the purpose of testing
        public ComboBox ComboAlignAgainst { get { return comboAlignAgainst; } }
        public DataGridView DataGridView { get { return dataGridView1; } }
        public ZedGraphControl RegressionGraph { get { return zedGraphControl; } }
        #endregion

        private void zedGraphControl_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt, ZedGraphControl.ContextMenuObjectState objState)
        {
            CopyEmfToolStripMenuItem.AddToContextMenu(sender, menuStrip);
        }
    }
}
