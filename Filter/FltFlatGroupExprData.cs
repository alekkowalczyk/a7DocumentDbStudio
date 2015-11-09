using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Linq;
using a7ExtensionMethods;
using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Utils;
using Newtonsoft.Json;

namespace a7DocumentDbStudio.Filter
{
    /// <summary>
    /// used now in a7Box for fields filter.
    /// A simple GroupFilter expression, that is flat, grouping only AtomicFilters (and not any filters) by one selected And/Join clause
    /// </summary>
    public class FltFlatGroupExprData : FilterExpressionData, INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// the and/or clause used to join the contained atomic filters
        /// </summary>
        public eAndOrJoin AndOr { get; set; }

        /// <summary>
        /// collection of atomic filters that are in this filter, the key of dictionary is the field names for 
        /// the atomicfilter in the value
        /// </summary>
        public Dictionary<string, FltAtomExprData> FieldFilters { get; set; }

        [JsonIgnore]
        public override bool HasActiveFilter
        {
            get
            {
                foreach (var flt in FieldFilters.Values)
                {
                    if (flt.IsActive)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Getting the atomicFilter value for the given as key fieldName.
        /// If filter for this fieldName not exist in this groupFilter, it is created during getting as an empty not-active atomicFilter with 'Contains' fieldOperator.
        /// Setting an value for the atomicFilter configured for the as key given fieldName.
        /// If on this fieldName no filter exist, a new one is created with the default 'Contains' fieldOperator.
        /// If the value is null or empty string, the filter for this value is set as not-active.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [JsonIgnore]
        public object this[string key]
        {
            get
            {
                return GetFilter(key).Value;
            }
            set
            {
                SetFieldFilter(key, value.ToStringAllowsNull());
            }
        }

        private string _commonValue;
        /// <summary>
        /// sets the value for all field filters that exist in this group filter at the moment of setting
        /// </summary>
        [JsonIgnore]
        public string CommonValue
        {
            get { return _commonValue; }
            set { 
                _commonValue = value;
                foreach (FltAtomExprData fa in FieldFilters.Values)
                {
                    this[fa.Field] = _commonValue;
                }
            }
        }

        public FltFlatGroupExprData()
        {

        }

        public void Reset()
        {
            foreach (FltAtomExprData fa in FieldFilters.Values)
            {
                this[fa.Field] = null;
            }
        }

        public FltFlatGroupExprData(eAndOrJoin andOr)
        {
            FieldFilters = new Dictionary<string, FltAtomExprData>();
            this.AndOr = andOr;
        }

        /// <summary>
        /// sets the field filter, if not exist, creates it, with default 'Contains' operator. If value is empty string or null or white space, deactivates the filter
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetFieldFilter(string fieldName, string value)
        {
            if (!FieldFilters.ContainsKey(fieldName))
            {
                FieldFilters[fieldName] = new FltAtomExprData()
                {
                     Field = fieldName,
                     Operator = FilterFieldOperator.Equal,
                };
            }
            FltAtomExprData fa = FieldFilters[fieldName];
            if (value.IsNotEmpty())
            {
                fa.IsActive = true;
                fa.Value = value;
            }
            else
            {
                fa.IsActive = false;
                fa.Value = "";
            }
        }

        public void SetFieldFilterPropertyType(string fieldName, PropertyType type)
        {
            if (!FieldFilters.ContainsKey(fieldName))
            {
                FieldFilters[fieldName] = new FltAtomExprData()
                {
                    Field = fieldName,
                    Operator = FilterFieldOperator.Equal,
                    IsActive = false
                };
            }
            FltAtomExprData fa = FieldFilters[fieldName];
            fa.PropertyType = type;
        }

        /// <summary>
        /// gets the filter for the given field name, if not existing creates a new not-active one with the default 'Contains' field operator
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public FltAtomExprData GetFilter(string fieldName)
        {
            if (!FieldFilters.ContainsKey(fieldName))
            {
                FieldFilters[fieldName] = new FltAtomExprData()
                {
                    Field = fieldName,
                    Operator = FilterFieldOperator.Equal,
                    Value = "",
                    IsActive = false
                };
            }
            return FieldFilters[fieldName];
        }

        /// <summary>
        /// clones instance of this filter with cloning of all instances of contained atomic filters
        /// </summary>
        /// <returns></returns>
        public override FilterExpressionData Clone()
        {
            FltFlatGroupExprData fsg = new FltFlatGroupExprData(this.AndOr);
            foreach (KeyValuePair<string, FltAtomExprData> kv in this.FieldFilters)
            {
                fsg.FieldFilters.Add(kv.Key, kv.Value.Clone() as FltAtomExprData);
            }
            fsg.Negate = this.Negate;
            return fsg;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        public override string ToString()
        {
            return "a7FltFlatGroupExprData:" + this.HasActiveFilter + ";" + this.AndOr.ToStringAllowsNull() + "," +
                   this.FieldFilters.Count;
        }

     

        public override System.Xml.Linq.XElement ToXml()
        {
            XElement ret = new XElement(XElementNames.FilterFlatExprNode,
                    new XAttribute("Operator", this.AndOr),
                    new XAttribute("Negate", this.Negate));
            foreach (var flt in this.FieldFilters.Values)
            {
                ret.Add(flt.ToXml());
            }
            return ret;
        }
    }
}
