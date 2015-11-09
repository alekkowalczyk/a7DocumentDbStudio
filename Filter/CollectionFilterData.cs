using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace a7DocumentDbStudio.Filter
{
    public class CollectionFilterData : ICloneable
    {
        public FltFlatGroupExprData FieldsFilter { get; set; }

        public FilterExpressionData AdvancedFilter { get; set; }

        [JsonIgnore]
        public FilterExpressionData JoinedFilter
        {
            get
            {
                var flt = Filter.And(FieldsFilter, AdvancedFilter);
                return flt;
            }
        }

        public CollectionFilterData()
        {
            FieldsFilter = new FltFlatGroupExprData(eAndOrJoin.And);
        }

        public CollectionFilterData Clone()
        {
            var ret = new CollectionFilterData();
            
            if(this.FieldsFilter != null)
                ret.FieldsFilter = this.FieldsFilter.Clone() as FltFlatGroupExprData;
            if (this.AdvancedFilter != null)
                ret.AdvancedFilter = this.AdvancedFilter.Clone();
            return ret;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
