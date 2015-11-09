using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;
using a7DocumentDbStudio.Utils;

namespace a7DocumentDbStudio.Filter
{
    /// <summary>
    /// and/or clause used by query builders
    /// </summary>
    public enum eAndOrJoin
    {
        And, Or
    }


    /// <summary>
    /// Group filter expression - grouping other filters (any that inherit from a7FitlerExprData, even other group filters)
    /// by set and/or clause. Is parsed by query builder
    /// </summary>
    public class FltGroupExprData : FilterExpressionData, ICloneable
    {
        /// <summary>
        /// the and/or clause used to join the contained filters
        /// </summary>
        public eAndOrJoin AndOr { get; set; }

        /// <summary>
        /// contained and joined by this group filter sub filters
        /// </summary>
        public List<FilterExpressionData> FilterExpressions { get; set; }

        public FltGroupExprData()
        {
            FilterExpressions = new List<FilterExpressionData>();
        }

        public FltGroupExprData(eAndOrJoin andOr)
        {
            this.AndOr = andOr;
            FilterExpressions = new List<FilterExpressionData>();
        }

        public FltGroupExprData(eAndOrJoin andOr, params FilterExpressionData[] filters)
        {
            this.AndOr = andOr;
            FilterExpressions = new List<FilterExpressionData>();
            foreach (FilterExpressionData flt in filters)
            {
                if(flt!=null)
                    FilterExpressions.Add(flt);
            }
        }

        public void ReplaceFilterInGroup(FilterExpressionData oldFilter, FilterExpressionData newFilter)
        {
            for (int i = 0; i < FilterExpressions.Count; i++)
            {
                if (FilterExpressions[i] == oldFilter)
                    FilterExpressions[i] = newFilter;
                if (FilterExpressions[i] is FltGroupExprData)
                {
                    ((FltGroupExprData)FilterExpressions[i]).ReplaceFilterInGroup(oldFilter, newFilter);
                }
            }
        }


        public void RemoveFilterFromGroup(FilterExpressionData Filter)
        {
            FilterExpressions.Remove(Filter);
            for (int i = 0; i < FilterExpressions.Count; i++)
            {
                if (FilterExpressions[i] is FltGroupExprData)
                {
                    ((FltGroupExprData)FilterExpressions[i]).RemoveFilterFromGroup(Filter);
                }
            }
        }

        /// <summary>
        /// cloning the instance of this and all sub filters
        /// </summary>
        /// <returns></returns>
        public override FilterExpressionData Clone()
        {
            FltGroupExprData thisFg = this as FltGroupExprData;
            FltGroupExprData fg = new FltGroupExprData(thisFg.AndOr);
            fg.AndOr = thisFg.AndOr;
            fg.Negate = this.Negate;
            foreach (FilterExpressionData f in thisFg.FilterExpressions)
            {
                if(f!=null)
                    fg.FilterExpressions.Add(f.Clone());
            }
            return fg;
        }

        public override bool HasActiveFilter
        {
            get
            {
                foreach (var flt in FilterExpressions)
                {
                    if (flt!=null && flt.HasActiveFilter)
                        return true;
                }
                return false;
            }
        }

        public override string ToString()
        {
            return "FltGroupExprData:" + this.HasActiveFilter + ";" + this.AndOr.ToString() + ";" +
                   this.FilterExpressions.Count;
        }

        public static FilterExpressionData ClearGroupFilter(FltGroupExprData groupFilter)
        {
            if (groupFilter.FilterExpressions.Count == 1)
                return groupFilter.FilterExpressions[0];

            for(int i = 0;i<groupFilter.FilterExpressions.Count;i++)
            {
                var gflt = groupFilter.FilterExpressions[i] as FltGroupExprData;
                groupFilter.FilterExpressions[i] = ClearGroupFilter(gflt);
            }
            return groupFilter;
        }

        public override System.Xml.Linq.XElement ToXml()
        {
            XElement ret = new XElement(XElementNames.FilterGroupExpressionNode,
                new XAttribute("Operator", this.AndOr),
                        new XAttribute("Negate", this.Negate));
            foreach (var flt in this.FilterExpressions)
            {
                ret.Add(flt.ToXml());
            }
            return ret;
        }
    }
}
