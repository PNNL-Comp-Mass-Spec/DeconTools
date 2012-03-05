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
        /// <param name="xmlFilename"></param>
        /// <returns></returns>
        public static WorkflowParameters CreateParameters(string xmlFilename)
        {
            Check.Require(File.Exists(xmlFilename), "Workflow parameter file could not be loaded. File not found.");
            XDocument doc = XDocument.Load(xmlFilename);
            var query = doc.Element("WorkflowParameters").Elements();

            Dictionary<string, string> parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                string paramName = item.Name.ToString();
                string paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }

            Globals.TargetedWorkflowTypes workflowType;

            bool successfulEnum = Enum.TryParse(parameterTableFromXML["WorkflowType"], out workflowType);

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


                    default:
                        workflowParameters = new BasicTargetedWorkflowParameters();
                        break;
                }

                

            }
            else
            {
                throw new System.ArgumentOutOfRangeException("Tried to create WorkflowParameter object. But WorkflowType is unknown.");
            }

            workflowParameters.LoadParameters(xmlFilename);

            return workflowParameters;

        }
  
        public virtual void LoadParameters(string xmlFilename)
        {
            Check.Require(File.Exists(xmlFilename), "Workflow parameter file could not be loaded. File not found.");
            XDocument doc = XDocument.Load(xmlFilename);
            var query = doc.Element("WorkflowParameters").Elements();

            Dictionary<string, string> parameterTableFromXML = new Dictionary<string, string>();
            foreach (var item in query)
            {
                string paramName = item.Name.ToString();
                string paramValue = item.Value;

                if (!parameterTableFromXML.ContainsKey(paramName))
                {
                    parameterTableFromXML.Add(paramName, paramValue);
                }
            }


            Type t = this.GetType();
            foreach (System.Reflection.MemberInfo mi in t.GetMembers().OrderBy(p => p.Name))
            {
                if (mi.MemberType == MemberTypes.Property)
                {
                    PropertyInfo pi = (PropertyInfo)mi;
                    string propertyName = pi.Name;

   
                    if (parameterTableFromXML.ContainsKey(propertyName))
                    {
                        Type propertyType = pi.PropertyType;

                        object value;

                        if (pi.CanWrite)
                        {
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

                      

                       
                    }
                    else
                    {
                        Console.WriteLine("Imported parameter values missing the Parameter: " + propertyName + ". Value was not updated. Current value= " + pi.GetValue(this, null));
                    }
                }
            }

        }

    

        public virtual void SaveParametersToXML(string xmlFilename)
        {

            StringBuilder sb = new StringBuilder();
            //Dictionary<string, string> parameterValues = GetParameterTable();

            Dictionary<string, object> parameterValues = GetParameterTable();


            XElement xml = new XElement("WorkflowParameters",
                from param in parameterValues
                select new XElement(param.Key, param.Value)
                    );

            xml.Save(xmlFilename);
        }


        public virtual string ToStringWithDetails()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, object> parameterValues = GetParameterTable();

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
            Dictionary<string, object> parameterValues = new Dictionary<string, object>();


            Type t = this.GetType();

            foreach (System.Reflection.MemberInfo mi in t.GetMembers().OrderBy(p => p.Name))
            {
                if (mi.MemberType == System.Reflection.MemberTypes.Property)
                {
                    System.Reflection.PropertyInfo pi = (PropertyInfo)mi;


                    string propertyName = pi.Name;
                    object propertyValue = pi.GetValue(this, null);

                    if (!parameterValues.ContainsKey(propertyName))
                    {
                        parameterValues.Add(propertyName, propertyValue);
                    }
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
