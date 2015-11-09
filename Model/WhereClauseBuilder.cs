using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using a7DocumentDbStudio.Filter;
using a7ExtensionMethods;
using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Utils;

namespace a7DocumentDbStudio.Model
{
    class WhereClauseBuilder
    {
        private FilterExpressionData _filter;
        private StringBuilder _whereStringBuilder;
        public Dictionary<string, object> Parameters { get; private set; }
        public string WhereClause { get; private set; }

        /// <summary>
        /// if collName is empty string no collName will be added to field names on where clause, and no relFilters will be replaced, simplified logic.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="collName"></param>
        public WhereClauseBuilder(FilterExpressionData filter, string collName = "")
        {
            _filter = filter;
            _whereStringBuilder = new StringBuilder();
            Parameters = new Dictionary<string, object>();
            filterExprAppendWhereAndParams(_filter, "", collName);
            WhereClause = _whereStringBuilder.ToString();
            if (WhereClause.IsNotEmpty())
                WhereClause = $" WHERE {WhereClause}";
        }

        private bool filterExprAppendWhereAndParams(FilterExpressionData filter, string preffixToAppend, string collName)
        {
            if (filter != null && filter.HasActiveFilter)
            {
                if (filter.Negate)
                    preffixToAppend += " NOT ";
                var fa = filter as FltAtomExprData;//atomic expr
                if (fa != null)
                {
                    string val = fa.Value;
                    appendFilterAtom2WhereSb(fa, preffixToAppend, collName);
                    return true;
                }
                var fg = filter as FltGroupExprData; //group expr
                if (fg != null)
                {
                    if (fg.FilterExpressions.Count == 1)
                        filterExprAppendWhereAndParams(fg.FilterExpressions[0], preffixToAppend, collName);
                    else
                        appendFilterGroup2WhereSb(fg, preffixToAppend, collName);
                    return true;
                }
                var fsg = filter as FltFlatGroupExprData;//simple group expr
                if (fsg != null)
                {
                    appendFilterSimpleGroup2WhereSb(fsg, preffixToAppend, collName);
                    return true;
                }
            }
            return true;
        }

    
        private void appendFilterSimpleGroup2WhereSb(FltFlatGroupExprData filterGroup, string preffixToAppend, string collName)
        {
            _whereStringBuilder.Append(preffixToAppend);
            if(filterGroup.FieldFilters.Where(kv=>kv.Value.HasActiveFilter).Count() >1)
                _whereStringBuilder.Append(" (");
            if (filterGroup.FieldFilters.Count == 1)
                filterExprAppendWhereAndParams(filterGroup.FieldFilters.Values.First<FltAtomExprData>(), "", collName);
            else
            {
                string sJoinOperator = " AND ";
                if (filterGroup.AndOr == eAndOrJoin.Or)
                    sJoinOperator = " OR ";
                bool isFirstActive = true;
                foreach (var flt in filterGroup.FieldFilters.Values)
                {
                    if (isFirstActive && flt.HasActiveFilter)
                    {
                        if (filterExprAppendWhereAndParams(flt, "", collName))
                            isFirstActive = false;
                    }
                    else
                        filterExprAppendWhereAndParams(flt, sJoinOperator, collName);
                }
            }
            if (filterGroup.FieldFilters.Where(kv => kv.Value.HasActiveFilter).Count() > 1)
                _whereStringBuilder.Append(")");
        }

        private void appendFilterGroup2WhereSb(FltGroupExprData filterGroup, string preffixToAppend, string collName)
        {
            _whereStringBuilder.Append(preffixToAppend);
            if(filterGroup.FilterExpressions.Where(f=>f.HasActiveFilter).Count() > 1)
                _whereStringBuilder.Append("(");
            if (filterGroup.FilterExpressions.Count == 1)
                filterExprAppendWhereAndParams(filterGroup.FilterExpressions[0], preffixToAppend, collName);
            else
            {
                string sJoinOperator = " AND ";
                if (filterGroup.AndOr == eAndOrJoin.Or)
                    sJoinOperator = " OR ";
                bool isFirstActive = true;
                for (int i = 0; i < filterGroup.FilterExpressions.Count; i++)
                {
                    var flt = filterGroup.FilterExpressions[i];
                    if (flt == null)
                        continue;
                    if (isFirstActive && flt.HasActiveFilter)
                    {
                        if (filterExprAppendWhereAndParams(flt, "", collName))
                            isFirstActive = false;
                    }
                    else
                        filterExprAppendWhereAndParams(flt, sJoinOperator, collName);
                }
            }
            if (filterGroup.FilterExpressions.Where(f => f.HasActiveFilter).Count() > 1)
                _whereStringBuilder.Append(")");
        }

        private void appendFilterAtom2WhereSb(FltAtomExprData filterAtom, string preffixToAppend, string collName)
        {
            if (filterAtom == null)
                return;
            var fltValue = filterAtom.Value;
            var isCustomValue = false;

            var pField = filterAtom.Field;// StringUtils.FirstCharacterToLower(filterAtom.Field);

            if (filterAtom.Operator == FilterFieldOperator.Between)
            {
                var filterBigger = filterAtom.Clone() as FltAtomExprData;
                filterBigger.Operator = FilterFieldOperator.GreaterEqualThan;
                filterBigger.Value = filterBigger.Value.BeforeString(";");
                var filterSmaller = filterAtom.Clone() as FltAtomExprData;
                filterSmaller.Operator = FilterFieldOperator.LessEqualThan;
                filterSmaller.Value = filterSmaller.Value.AfterString(";");
                var group = Filter.Filter.And(filterBigger, filterSmaller);
                filterExprAppendWhereAndParams(group, preffixToAppend, collName);
                return;
            }

            if (filterAtom.Operator == FilterFieldOperator.In && (filterAtom.Values == null || filterAtom.Values.Count == 0) && filterAtom.Value.IsEmpty())
            {
                filterAtom = Filter.Filter.AlwaysFalse();
            }
            if (filterAtom.Field == "1")
            {
                _whereStringBuilder.Append(preffixToAppend);
                _whereStringBuilder.Append(" (1=" + filterAtom.Value + ")");
                return;
            }

            if (pField == null)
            {
                filterAtom.Field = "";
                filterAtom.Operator = FilterFieldOperator.IsNull;
            }
            _whereStringBuilder.Append(preffixToAppend);
            _whereStringBuilder.Append("(");

         

            var fld = pField;

            if (filterAtom.Operator != FilterFieldOperator.Contains &&
                filterAtom.Operator != FilterFieldOperator.StartsWith &&
                filterAtom.Operator != FilterFieldOperator.EndsWith)
            {
                if (collName.IsNotEmpty())
                {
                    _whereStringBuilder.Append(collName);
                    _whereStringBuilder.Append(".");
                }
                _whereStringBuilder.Append(fld);
            }
            else
            {
                fld = $"{collName}.{fld}";
            }

            _whereStringBuilder.Append(" ");
            switch (filterAtom.Operator)
            {
                case FilterFieldOperator.Equal:
                    _whereStringBuilder.Append("="); break;
                case FilterFieldOperator.GreaterThan:
                    _whereStringBuilder.Append(">"); break;
                case FilterFieldOperator.GreaterEqualThan:
                    _whereStringBuilder.Append(">="); break;
                case FilterFieldOperator.In:
                    _whereStringBuilder.Append("IN"); break;
                case FilterFieldOperator.LessThan:
                    _whereStringBuilder.Append("<"); break;
                case FilterFieldOperator.LessEqualThan:
                    _whereStringBuilder.Append("<="); break;
                //In document DB it's different
                //case FilterFieldOperator.Like:
                //case FilterFieldOperator.Contains:
                //case FilterFieldOperator.StartsWith:
                //case FilterFieldOperator.EndsWith:
                //    _whereStringBuilder.Append("LIKE"); break;
                case FilterFieldOperator.NotEqual:
                    _whereStringBuilder.Append("<>"); break;
                case FilterFieldOperator.IsNull:
                    _whereStringBuilder.Append("IS NULL"); break;
                case FilterFieldOperator.IsNotNull:
                    _whereStringBuilder.Append("IS NOT NULL"); break;
            }
            _whereStringBuilder.Append(" ");
            if (filterAtom.Operator == FilterFieldOperator.In)
            {
                _whereStringBuilder.Append("(");
            }

            if (filterAtom.Operator == FilterFieldOperator.In)
            {
                List<string> valuesList = new List<string>();
                if (filterAtom.Values != null)
                    valuesList = filterAtom.Values;
                else
                    valuesList = filterAtom.Value.SplitCharList(';');

                for (int i = 0; i < valuesList.Count; i++)
                {
                    if (i > 0)
                        _whereStringBuilder.Append(",");
                    _whereStringBuilder.Append("@");
                    _whereStringBuilder.Append(FieldName2ParamName(pField + i, filterAtom));
                    Parameters.Add(FieldName2ParamName(pField + i, filterAtom), valuesList[i]);
                }
            }
            else if(filterAtom.Operator == FilterFieldOperator.StartsWith ||
                filterAtom.Operator == FilterFieldOperator.EndsWith ||
                filterAtom.Operator == FilterFieldOperator.Contains)
            {
                var func = "";
                if (filterAtom.Operator == FilterFieldOperator.StartsWith)
                    func = "STARTSWITH";
                else if (filterAtom.Operator == FilterFieldOperator.EndsWith)
                    func = "ENDSWITH";
                else if (filterAtom.Operator == FilterFieldOperator.Contains)
                    func = "CONTAINS";
                _whereStringBuilder.Append($" {func}({fld},@{FieldName2ParamName(pField, filterAtom)}) ");
                Parameters[FieldName2ParamName(pField, filterAtom)] = fltValue;
            }
            else if (filterAtom.Operator != FilterFieldOperator.IsNull && filterAtom.Operator != FilterFieldOperator.IsNotNull)
            {
                if (!isCustomValue)
                {
                    _whereStringBuilder.Append("@");
                    _whereStringBuilder.Append(FieldName2ParamName(pField, filterAtom));

                    object value = fltValue;

                    if (filterAtom.PropertyType == PropertyType.Integer)
                    {
                        var isNumberTry = fltValue.ToInt(int.MaxValue);
                        if (isNumberTry != int.MaxValue)
                            value = isNumberTry;
                    }
                    else if(filterAtom.PropertyType == PropertyType.Float)
                    {
                        var isNumberTry = fltValue.ToDouble(double.NaN);
                        if (isNumberTry != double.NaN)
                            value = isNumberTry;
                    }

                    Parameters[FieldName2ParamName(pField, filterAtom)] = value;
                }
                else
                {
                    fltValue = fltValue.Replace("'", "''");
                    _whereStringBuilder.Append(fltValue);
                }
            }

            if (filterAtom.Operator == FilterFieldOperator.In)
            {
                _whereStringBuilder.Append(")");
            }
            _whereStringBuilder.Append(")");
        }

        private string FieldName2ParamName(string fieldName, FltAtomExprData atomic)
        {
            return fieldName.Replace(".", "") + atomic.GetHashCode().ToString();
        }
    }
}
