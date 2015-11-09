using a7DocumentDbStudio.ViewModel;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Controls.FilterEditor
{
    class FilterEditorUtils
    {
        public static IEnumerable<FilterElementDefinition> GetFilterEditorElements(CollectionVM collection)
        {
            if (collection != null)
            {
                var retList = new List<FilterElementDefinition>();
                foreach(var prop in collection.AvailableProperties)
                {
                    retList.Add(FilterElementDefinition.GetFieldFilterElement(prop));
                }
                return retList;
            }
            return new List<FilterElementDefinition>();
        }
    }
}
