using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace a7DocumentDbStudio.Filter
{
    /// <summary>
    /// base class for all Filter Expressions that are parsed to WHERE clauses by QueryBuilders
    /// </summary>
    public abstract class FilterExpressionData : ICloneable
    {
        public bool Negate { get; set; }

        public static FilterExpressionData operator &(FilterExpressionData f1, FilterExpressionData f2)
        {
            return Filter.And(f1, f2);
        }

        public static FilterExpressionData operator |(FilterExpressionData f1, FilterExpressionData f2)
        {
            return Filter.Or(f1, f2);
        }

        [JsonIgnore]
        public abstract bool HasActiveFilter { get; }
        
        public abstract XElement ToXml();

        private string _xmlString;
        [JsonIgnore]
        public string XmlString
        {
            get
            {
                if (_xmlString == null)
                {
                    var x = this.ToXml();
                    if(x!=null)
                        _xmlString = x.ToString();
                }
                return _xmlString;
            }
        }

        /// <summary>
        /// cloning the instance of this and all sub filters if exist
        /// </summary>
        /// <returns></returns>
        public abstract FilterExpressionData Clone();

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
