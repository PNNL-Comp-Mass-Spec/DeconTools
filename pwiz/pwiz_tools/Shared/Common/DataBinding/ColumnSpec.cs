﻿/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2011 University of Washington - Seattle, WA
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using pwiz.Common.Collections;

namespace pwiz.Common.DataBinding
{
    [XmlRoot("column")]
    public class ColumnSpec
    {
        public ColumnSpec()
        {
        }
        public ColumnSpec(IdentifierPath identifierPath)
        {
            Name = identifierPath.ToString();
        }
        public ColumnSpec(ColumnSpec that)
        {
            Name = that.Name;
            Caption = that.Caption;
            Format = that.Format;
            Hidden = that.Hidden;
            SortIndex = that.SortIndex;
            SortDirection = that.SortDirection;
        }
        public string Name { get; private set; }
        public ColumnSpec SetName(string value)
        {
            return new ColumnSpec(this){Name = value};
        }
        public string Caption { get; private set; }
        public ColumnSpec SetCaption(string value)
        {
            return new ColumnSpec(this){Caption = value};
        }
        public string Format { get; private set; }
        public ColumnSpec SetFormat(string value)
        {
            return new ColumnSpec(this){Format = value};
        }
        public bool Hidden { get; private set; }
        public ColumnSpec SetHidden(bool value)
        {
            return new ColumnSpec(this) {Hidden = value};
        }
        public int? SortIndex { get; private set; }
        public ColumnSpec SetSortIndex(int? value)
        {
            return new ColumnSpec(this){SortIndex = value};
        }
        public ListSortDirection? SortDirection { get; private set; }
        public ColumnSpec SetSortDirection(ListSortDirection? value)
        {
            return new ColumnSpec(this){SortDirection = value};
        }

        [XmlIgnore]
        public IdentifierPath IdentifierPath
        {
            get { return IdentifierPath.Parse(Name); }
        }
        public ColumnSpec SetIdentifierPath(IdentifierPath value)
        {
            return new ColumnSpec(this) {Name = value == null ? "" : value.ToString()};
        }
        public static ColumnSpec ReadXml(XmlReader reader)
        {
            var columnSpec = new ColumnSpec();
            columnSpec.Name = reader.GetAttribute("name");
            columnSpec.Caption = reader.GetAttribute("caption");
            columnSpec.Format = reader.GetAttribute("format");
            columnSpec.Hidden = "true" == reader.GetAttribute("hidden");
            string sortIndex = reader.GetAttribute("sortindex");
            if (sortIndex != null)
            {
                columnSpec.SortIndex = Convert.ToInt32(sortIndex);
            }
            string sortDirection = reader.GetAttribute("sortdirection");
            if (sortDirection != null)
            {
                columnSpec.SortDirection = (ListSortDirection) Enum.Parse(typeof(ListSortDirection), sortDirection);
            }
            
            
            bool empty = reader.IsEmptyElement;
            reader.ReadElementString("column");
            if (!empty)
            {
                reader.ReadEndElement();
            }
            return columnSpec;
        }

        public void WriteXml(XmlWriter writer)
        {
            if (Name != null)
            {
                writer.WriteAttributeString("name", Name);
            }
            if (Caption != null)
            {
                writer.WriteAttributeString("caption", Caption);
            }
            if (Format != null)
            {
                writer.WriteAttributeString("format", Format);
            }
            if (Hidden)
            {
                writer.WriteAttributeString("hidden", "true");
            }
            if (SortIndex != null)
            {
                writer.WriteAttributeString("sortindex", SortIndex.ToString());
            }
            if (SortDirection != null)
            {
                writer.WriteAttributeString("sortdirection", SortDirection.ToString());
            }
        }

        public bool Equals(ColumnSpec other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) 
                && Equals(other.Caption, Caption) 
                && Equals(other.Format, Format)
                && Equals(other.Hidden, Hidden)
                && Equals(other.SortIndex, SortIndex)
                && Equals(other.SortDirection, SortDirection);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ColumnSpec)) return false;
            return Equals((ColumnSpec) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Name.GetHashCode();
                result = (result*397) ^ (Caption != null ? Caption.GetHashCode() : 0);
                result = (result*397) ^ (Format != null ? Format.GetHashCode() : 0);
                result = (result*397) ^ Hidden.GetHashCode();
                result = (result*397) ^ (SortIndex != null ? SortIndex.GetHashCode() : 0);
                result = (result*397) ^ (SortDirection != null ? SortDirection.GetHashCode() : 0);
                return result;
            }
        }
    }
    public class FilterSpec
    {
        public FilterSpec()
        {
        }
        public FilterSpec(IdentifierPath identifierPath, IFilterOperation filterOperation, string operand)
        {
            Column = identifierPath.ToString();
            OpName = filterOperation == null ? null : filterOperation.OpName;
            Operand = operand;
        }
        public FilterSpec(FilterSpec that)
        {
            Column = that.Column;
            OpName = that.OpName;
            Operand = that.Operand;
        }
        public string Column { get; private set; }
        public FilterSpec SetColumn(string column)
        {
            return new FilterSpec(this){Column = column};
        }
        public IdentifierPath ColumnId { get { return IdentifierPath.Parse(Column); } }
        public FilterSpec SetColumnId(IdentifierPath columnId)
        {
            return SetColumn(columnId.ToString());
        }
        public string OpName { get; private set; }
        public FilterSpec SetOp(string op)
        {
            return new FilterSpec(this){OpName = op};
        }
        public IFilterOperation Operation {get { return FilterOperations.GetOperation(OpName);}}
        public FilterSpec SetOperation(IFilterOperation operation)
        {
            return SetOp(operation == null ? "" : operation.OpName);
        }
        public string Operand { get; private set; }
        public FilterSpec SetOperand(string operand)
        {
            return new FilterSpec(this){Operand = operand};
        }
        public static FilterSpec ReadXml(XmlReader reader)
        {
            var filterSpec = new FilterSpec();
            filterSpec.Column = reader.GetAttribute("column");
            filterSpec.OpName = reader.GetAttribute("opname");
            filterSpec.Operand = reader.GetAttribute("operand");
            bool empty = reader.IsEmptyElement;
            reader.ReadElementString("filter");
            if (!empty)
            {
                reader.ReadEndElement();
            }
            return filterSpec;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("column", Column);
            writer.WriteAttributeString("opname", OpName);
            if (Operand != null)
            {
                writer.WriteAttributeString("operand", Operand);
            }
        }

        public bool Equals(FilterSpec other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Column, Column) && Equals(other.OpName, OpName) && Equals(other.Operand, Operand);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(FilterSpec)) return false;
            return Equals((FilterSpec)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Column == null ? 0 : Column.GetHashCode();
                result = (result*397) ^ (OpName == null ? 0 : OpName.GetHashCode());
                result = (result*397) ^ (Operand == null ? 0 : Operand.GetHashCode());
                return result;
            }
        }

    }

    /// <summary>
    /// Models a user's customization of a view.
    /// A view has a list of columns to display.  It also can have a filter and sort to be applied (NYI).
    /// </summary>
    public class ViewSpec : IComparable<ViewSpec>
    {
        public ViewSpec()
        {
            Columns = new ColumnSpec[0];
            Filters = new FilterSpec[0];
        }
        public ViewSpec(ViewSpec that)
        {
            Name = that.Name;
            Columns = that.Columns;
            Filters = that.Filters;
            SublistName = that.SublistName;
        }
        public string Name { get; private set; }
        public ViewSpec SetName(string value)
        {
            return new ViewSpec(this){Name = value};
        }
        public IList<ColumnSpec> Columns { get; private set; }
        public ViewSpec SetColumns(IEnumerable<ColumnSpec> value)
        {
            return new ViewSpec(this)
                       {
                           Columns = Array.AsReadOnly(value.ToArray())
                       };
        }
        public IList<FilterSpec> Filters { get; private set; }
        public ViewSpec SetFilters(IEnumerable<FilterSpec> value)
        {
            return new ViewSpec(this){Filters = Array.AsReadOnly(value.ToArray())};
        }
        public string SublistName { get; private set; }
        public IdentifierPath SublistId
        {
            get { return IdentifierPath.Parse(SublistName); }
            set { SublistName = value.ToString(); }
        }
        public ViewSpec SetSublistId(IdentifierPath sublistId)
        {
            return new ViewSpec(this){SublistId = sublistId};
        }
        public static ViewSpec ReadXml(XmlReader reader)
        {
            var viewSpec = new ViewSpec();
            viewSpec.Name = reader.GetAttribute("name");
            viewSpec.SublistName = reader.GetAttribute("sublist");
            var columns = new List<ColumnSpec>();
            var filters = new List<FilterSpec>();
            if (reader.IsEmptyElement)
            {
                reader.ReadElementString("view");
                return viewSpec;
            }
            reader.Read();
            while (true)
            {
                if (reader.IsStartElement("column"))
                {
                    columns.Add(ColumnSpec.ReadXml(reader));
                }
                else if (reader.IsStartElement("filter"))
                {
                    filters.Add(FilterSpec.ReadXml(reader));
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                    break;
                }
                else
                {
                    reader.Read();
                }
            }
            viewSpec.Columns = Array.AsReadOnly(columns.ToArray());
            viewSpec.Filters = Array.AsReadOnly(filters.ToArray());
            return viewSpec;
        }

        public void WriteXml(XmlWriter writer)
        {
            if (Name != null)
            {
                writer.WriteAttributeString("name", Name);
            }
            if (SublistName != null)
            {
                writer.WriteAttributeString("sublist", SublistName);
            }
            foreach (var column in Columns)
            {
                writer.WriteStartElement("column");
                column.WriteXml(writer);
                writer.WriteEndElement();
            }
            foreach (var filter in Filters)
            {
                writer.WriteStartElement("filter");
                filter.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        public bool Equals(ViewSpec other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name)
                   && Columns.SequenceEqual(other.Columns)
                   && Filters.SequenceEqual(other.Filters)
                   && SublistId.Equals(other.SublistId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ViewSpec)) return false;
            return Equals((ViewSpec) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result*397) ^ CollectionUtil.GetHashCodeDeep(Columns);
                result = (result * 397) ^ CollectionUtil.GetHashCodeDeep(Filters);
                result = (result*397) ^ SublistId.GetHashCode();
                return result;
            }
        }

        public int CompareTo(ViewSpec other)
        {
            return CaseInsensitiveComparer.Default.Compare(Name, other.Name);
        }
    }

    [XmlRoot("views")]
    public class ViewSpecList : IXmlSerializable
    {
        public ViewSpecList()
        {
            ViewSpecs = new ViewSpec[0];
        }
        public string Name { get; set; }
        public IList<ViewSpec> ViewSpecs { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.GetAttribute("name");
            if (reader.IsEmptyElement)
            {
                reader.ReadElementString("views");
                return;
            }
            reader.Read();
            var viewSpecs = new List<ViewSpec>();
            while (true)
            {
                if (reader.IsStartElement("view"))
                {
                    viewSpecs.Add(ViewSpec.ReadXml(reader));
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                    break;
                }
                else
                {
                    reader.Read();
                }
            }
            ViewSpecs = viewSpecs.AsReadOnly();
        }

        public void WriteXml(XmlWriter writer)
        {
            if (Name != null)
            {
                writer.WriteAttributeString("name", Name);
            }
            foreach (var viewSpec in ViewSpecs)
            {
                writer.WriteStartElement("view");
                viewSpec.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }
}
