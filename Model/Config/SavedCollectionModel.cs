using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model.Config
{
    public  class SavedCollectionModel
    {
        public bool IsExpanded { get; set; }
        public string Name { get; set; }
        public List<CollectionViewModel> Views { get; set; }

        public SavedCollectionModel()
        {
            Views = new List<CollectionViewModel>();
        }
    }
}
