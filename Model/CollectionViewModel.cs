using a7DocumentDbStudio.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    /// <summary>
    /// To be clear - it's not an "Collection ViewModel", it's an "CollectionView Model" :-)
    /// </summary>
    public class CollectionViewModel
    {
        public string ViewName { get; set; }
        public string CollectionName { get; set; }
        public CollectionFilterData Filter { get; set; }
        public bool IsSqlEditMdoe { get; set; }
        public bool IsSqlVisible { get; set; }
        public string SqlText { get; set; }
        public List<PropertyDefinitionModel> Columns { get; set; }

        public CollectionViewModel()
        {

        }

        public CollectionViewModel Clone()
         => new CollectionViewModel
         {
             ViewName = this.ViewName,
             CollectionName = this.CollectionName,
             Filter = this.Filter.Clone(),
             IsSqlEditMdoe = this.IsSqlEditMdoe,
             IsSqlVisible = this.IsSqlVisible,
             Columns = new List<PropertyDefinitionModel>(this.Columns),
             SqlText = this.SqlText
         };
    }
}
