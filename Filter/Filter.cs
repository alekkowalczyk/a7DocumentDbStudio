using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Utils;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace a7DocumentDbStudio.Filter
{
    public class Filter
    {
        #region static functions

       
        /// <summary>
        /// create a simple equal operator atomic filter that will parse by query builder to a "[field]='value'" clause
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterExpressionData Equal(string field, object value)
        {
            return new FltAtomExprData(field, FilterFieldOperator.Equal, value.ToStringAllowsNull());
        }

        public static FilterExpressionData GreatherThan(string field, object value)
        {
            return new FltAtomExprData(field, FilterFieldOperator.GreaterThan, value.ToStringAllowsNull());
        }

        public static FilterExpressionData LessThan(string field, object value)
        {
            return new FltAtomExprData(field, FilterFieldOperator.LessThan, value.ToStringAllowsNull());
        }

        public static FilterExpressionData LessEqualThan(string field, object value)
        {
            return new FltAtomExprData(field, FilterFieldOperator.LessEqualThan, value.ToStringAllowsNull());
        }

        public static FilterExpressionData IsNull(string field)
        {
            return new FltAtomExprData(field, FilterFieldOperator.IsNull, "");
        }

        public static FilterExpressionData IsNotNull(string field)
        {
            return new FltAtomExprData(field, FilterFieldOperator.IsNotNull, "");
        }

        public static FilterExpressionData In(string field, List<string> values)
        {
            return new FltAtomExprData(field, FilterFieldOperator.In, values);
        }

        public static FilterExpressionData In(string field, List<object> values)
        {
            List<string> sValues = new List<string>();
            if (values != null)
            {
                foreach (var longValue in values)
                {
                    sValues.Add(longValue.ToStringAllowsNull());
                }
            }
            return new FltAtomExprData(field, FilterFieldOperator.In, sValues) { IsActive = (values != null) };
        }

        public static FilterExpressionData In(string field, List<long> values)
        {
            List<string> sValues = new List<string>();
            if (values != null)
            {
                foreach (var longValue in values)
                {
                    sValues.Add(longValue.ToString());
                }
            }
            return new FltAtomExprData(field, FilterFieldOperator.In, sValues) { IsActive = (values!=null)};
        }

        /// <summary>
        /// create a simple like operator atomic filter that will parse by query builder to a "[field] LIKE('value')" clause
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterExpressionData Like(string field, string value)
        {
            return new FltAtomExprData(field, FilterFieldOperator.Like, value);
        }

        /// <summary>
        /// create a group filter that will parse by query builder to a "filter1 [andOr] filter2 [andOr] filter3...." clause
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterExpressionData Group(eAndOrJoin andOr, params FilterExpressionData[] filters)
        {
            return new FltGroupExprData(andOr, filters);
        }

        /// <summary>
        /// create a group filter grouping by AND that will parse by query builder to a "filter1 AND filter2 AND filter3...." clause,
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static FilterExpressionData And(params FilterExpressionData[] filters)
        {
            return JoinFilters(eAndOrJoin.And, filters);
        }

        public static FilterExpressionData Or(params FilterExpressionData[] filters)
        {
            return JoinFilters(eAndOrJoin.Or, filters);
        }

        public static FltAtomExprData AlwaysFalse()
        {
            return new FltAtomExprData("1", FilterFieldOperator.Equal, "2");
        }

        public static FltAtomExprData AlwaysTrue()
        {
            return new FltAtomExprData("1", FilterFieldOperator.Equal, "1");
        }

        public static FilterExpressionData JoinFilters(eAndOrJoin andOrJoin, params FilterExpressionData[] filters)
        {
            List<FilterExpressionData> lFilters = filters.Distinct<FilterExpressionData>().ToList<FilterExpressionData>();
            lFilters.RemoveAll(item => item == null);
            if (lFilters.Count == 0)
                return new FltGroupExprData(andOrJoin); //return empty and group filter
            if (lFilters.Count == 1)
                return lFilters[0].Clone(); //return the only provided filter
            FltGroupExprData groupFilter = null;

            foreach (var flt in lFilters)//check if in collection already exist an and filter
            {
                if (flt is FltGroupExprData)
                {
                    groupFilter = flt as FltGroupExprData;
                    if (groupFilter.AndOr == andOrJoin)
                        break;
                    else
                        groupFilter = null;
                }
            }
            if (groupFilter == null)
                return Filter.And(new FltGroupExprData(andOrJoin,
                    lFilters.ToArray()));
            else //if yes, add to him the rest expression and return it.
            {
                lFilters.Remove(groupFilter);
                groupFilter = groupFilter.Clone() as FltGroupExprData;
                foreach (var flt in lFilters)
                {
                    if (!groupFilter.FilterExpressions.Contains(flt))
                        groupFilter.FilterExpressions.Add(flt.Clone());
                }
                return groupFilter;
            }
        }

        public static FilterExpressionData GroupEqual(eAndOrJoin join, params string[] fieldValues)
        {
            var flt = new FltGroupExprData(join);
            string field = "";
            foreach (var fv in fieldValues)
            {
                if (field == "")
                {
                    field = fv;

                }
                else
                {
                    flt.FilterExpressions.Add(Filter.Equal(field, fv));
                    field = "";
                }
            }
            return flt;
        }

        public static FilterExpressionData AndEqualFilters(params string[] fieldValues)
        {
            return GroupEqual(eAndOrJoin.And, fieldValues);
        }

        public static FilterExpressionData OrEqualFilters(params string[] fieldValues)
        {
            return GroupEqual(eAndOrJoin.Or, fieldValues);
        }

        #endregion

        public static FilterExpressionData IsActive
        {
            get
            {
                return Filter.Equal("IsActive", true);
            }
        }

        public static FilterExpressionData IsNotActive
        {
            get
            {
                return Filter.Equal("IsActive", false);
            }
        }

        public static FilterExpressionData FromXml(string xml)
        {
            if (xml != null && xml != "")
            {
                return FromXml(XElement.Parse(xml));
            }
            return null;
        }

        public static FilterExpressionData FromXml(XElement xml)
        {
            if (xml != null && xml.Name.LocalName.Equals(XElementNames.FilterAtomExpressionNode))
            {
                return fromXmlAtom(xml);
            }
            else if (xml.Name.LocalName.Equals(XElementNames.FilterGroupExpressionNode))
            {
                return fromXmlGroup(xml);
            } 
            else if (xml.Name.LocalName.Equals(XElementNames.FilterFalseExprNode))
            {
                return FltAtomExprData.AlwaysFalse();
            }
            else if (xml.Name.LocalName.Equals(XElementNames.FilterTrueExprNode))
            {
                return FltAtomExprData.AlwaysTrue();
            }
            return null;
        }

        private static FilterExpressionData fromXmlAtom(XElement xml)
        {
            FltAtomExprData ret = xml.ToObject<FltAtomExprData>
                                    (
                                        new a7MappingXElement2Property("Operator", "Operator",
                                            (s) =>
                                            {
                                                FilterFieldOperator eOp = FilterFieldOperator.Equal;
                                                Enum.TryParse<FilterFieldOperator>(s, true, out eOp);
                                                return eOp;
                                            }
                                            ),
                                        new a7MappingXElement2Property("Negate", "Negate", (s) => s.ToBool(false))
                                    );
            ret.IsActive = true;
            return ret;
        }

        private static FilterExpressionData fromXmlGroup(XElement xml)
        {
            eAndOrJoin andor = xml.AttributeAsString("Operator").ToEnum<eAndOrJoin>(eAndOrJoin.And);
            FltGroupExprData flt = new FltGroupExprData(andor);
            flt.Negate = xml.AttributeAsBool("Negate", false);
            foreach (var sub in xml.Elements())
            {
                flt.FilterExpressions.Add(FromXml(sub));
            }
            return flt;
        }


    }
}
