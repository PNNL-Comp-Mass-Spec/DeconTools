using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using DeconTools.Utilities;

namespace DeconTools.Workflows.Backend.Core
{
    public abstract class WorkflowParameters
    {

        #region Constructors
        #endregion

        #region Properties
        public abstract Globals.TargetedWorkflowTypes WorkflowType { get; }



        #endregion

        #region Public Methods

        /// <summary>
        /// A factory method for creating the WorkflowParameters class. Based on the 'WorkflowType' parameter of the xml file.
        /// </summary>
        /// <param name="xmlFilepath"></param>
        /// <returns></returns>
        public static WorkflowParameters CreateParameters(string xmlFilepath)
        {
            Check.Require(File.Exists(xmlFilepath), "Workflow parameter file could not be loaded. File not found: " + xmlFilepath);
            var doc = XDocument.Load(xmlFilepath);
            var xElement = doc.Element("WorkflowParameters");

            if (xElement == null)
            {
                throw new Exception("WorkflowParameters element not found in " + xmlFilepath);
            }

            var query = xElement.Elements();

            var parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                var paramName = item.Name.ToString();
                var paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }

            Globals.TargetedWorkflowTypes workflowType;

            var successfulEnum = Enum.TryParse(parameterTableFromXML["WorkflowType"], out workflowType);

            WorkflowParameters workflowParameters;
            if (successfulEnum)
            {
                switch (workflowType)
                {
                    case Globals.TargetedWorkflowTypes.Undefined:
                        workflowParameters = new BasicTargetedWorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.UnlabelledTargeted1:
                        workflowParameters = new BasicTargetedWorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.O16O18Targeted1:
                        workflowParameters = new O16O18WorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.N14N15Targeted1:
                        workflowParameters = new N14N15Workflow2Parameters();
                        break;
                    case Globals.TargetedWorkflowTypes.SipperTargeted1:
                        workflowParameters = new SipperTargetedWorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.TargetedAlignerWorkflow1:
                        workflowParameters = new TargetedAlignerWorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.BasicTargetedWorkflowExecutor1:
                        workflowParameters = new BasicTargetedWorkflowExecutorParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.LcmsFeatureTargetedWorkflowExecutor1:
                        workflowParameters = new LcmsFeatureTargetedWorkflowExecutorParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.SipperWorkflowExecutor1:
                        workflowParameters = new SipperWorkflowExecutorParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.TopDownTargeted1:
                        workflowParameters = new TopDownTargetedWorkflowParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.TopDownTargetedWorkflowExecutor1:
                        workflowParameters = new TopDownTargetedWorkflowExecutorParameters();
                        break;
                    case Globals.TargetedWorkflowTypes.UIMFTargetedMSMSWorkflowCollapseIMS:
                        workflowParameters = new UIMFTargetedMSMSWorkflowCollapseIMSParameters();
                        break;
                    default:
                        workflowParameters = new BasicTargetedWorkflowParameters();
                        break;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "Tried to create WorkflowParameter object. But WorkflowType is unknown.");
            }

            workflowParameters.LoadParameters(xmlFilepath);

            return workflowParameters;

        }

        public virtual void LoadParameters(string xmlFilepath)
        {
            Check.Require(File.Exists(xmlFilepath), "Workflow parameter file could not be loaded. File not found: " + xmlFilepath);
            var doc = XDocument.Load(xmlFilepath);
            var xElement = doc.Element("WorkflowParameters");
            
            if (xElement == null)
            {
                throw new Exception("WorkflowParameters element not found in " + xmlFilepath);
            }

            var query = xElement.Elements();

            var parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                var paramName = item.Name.ToString();
                var paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }


            var t = GetType();
            foreach (var mi in t.GetMembers().OrderBy(p => p.Name))
            {
                if (mi.MemberType != MemberTypes.Property)
                {
                    continue;
                }

                var pi = (PropertyInfo)mi;
                var propertyName = pi.Name;


                if (parameterTableFromXML.ContainsKey(propertyName))
                {
                    var propertyType = pi.PropertyType;

                    if (!pi.CanWrite)
                    {
                        continue;
                    }

                    object value;
                    if (propertyType.IsEnum)
                    {
                        //value = Enum.ToObject(propertyType
                        value = Enum.Parse(propertyType, parameterTableFromXML[propertyName], true);

                    }
                    else
                    {
                        value = Convert.ChangeType(parameterTableFromXML[propertyName], propertyType);
                    }

                    pi.SetValue(this, value, null);
                }
                else
                {
                    var shortFilename = Path.GetFileName(xmlFilepath);
                    Console.WriteLine("xml file: " + shortFilename + "; missing parameter: " + propertyName + ". Using default value: " + pi.GetValue(this, null));
                }
            }
        }



        public virtual void SaveParametersToXML(string xmlFilename)
        {

            var parameterValues = GetParameterTable();


            var xml = new XElement("WorkflowParameters",
                from param in parameterValues
                select new XElement(param.Key, param.Value)
                    );

            xml.Save(xmlFilename);
        }


        public virtual string ToStringWithDetails()
        {
            var sb = new StringBuilder();
            var parameterValues = GetParameterTable();

            foreach (var item in parameterValues)
            {
                sb.Append(item.Key);
                sb.Append("\t");
                sb.Append(item.Value);
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();

        }



        public Dictionary<string, object> GetParameterTable()
        {
            var parameterValues = new Dictionary<string, object>();

            var t = GetType();

            foreach (var mi in t.GetMembers().OrderBy(p => p.Name))
            {
                if (mi.MemberType != MemberTypes.Property)
                {
                    continue;
                }

                var pi = (PropertyInfo)mi;

                var propertyName = pi.Name;
                var propertyValue = pi.GetValue(this, null);

                if (!parameterValues.ContainsKey(propertyName))
                {
                    parameterValues.Add(propertyName, propertyValue);
                }
            }

            return parameterValues;

        }

        //public Dictionary<string, string> GetParameterTable()
        //{

        //    Dictionary<string, string> parameterValues = new Dictionary<string, string>();


        //    Type t = this.GetType();

        //    foreach (System.Reflection.MemberInfo mi in t.GetMembers().OrderBy(p => p.Name))
        //    {
        //        if (mi.MemberType == System.Reflection.MemberTypes.Property)
        //        {
        //            System.Reflection.PropertyInfo pi = (PropertyInfo)mi;


        //            string propertyName = pi.Name;
        //            string propertyValue = pi.GetValue(this, null).ToString();

        //            if (!parameterValues.ContainsKey(propertyName))
        //            {
        //                parameterValues.Add(propertyName, propertyValue);
        //            }
        //        }
        //    }

        //    return parameterValues;


        //}



        #endregion

        #region Private Methods

        #endregion

    }
}
