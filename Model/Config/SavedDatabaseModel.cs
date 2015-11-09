using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model.Config
{
    public class SavedDatabaseModel
    {
        public bool IsExpanded { get; set; }
        public string Name { get; set; }
        public List<SavedCollectionModel> Collections { get; set; }

        public SavedDatabaseModel()
        {
            Collections = new List<SavedCollectionModel>();
        }

        public SavedCollectionModel GetCollection(string collName)
        {
            var coll = Collections.FirstOrDefault(c => c.Name == collName);
            if (coll == null)
            {
                coll = new SavedCollectionModel { Name = collName };
                Collections.Add(coll);
            }
            return coll;
        }
    }
}
