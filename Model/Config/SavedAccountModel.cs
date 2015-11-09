using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model.Config
{
    public class SavedAccountModel
    {
        public AccountCredentialsModel Credentials { get; set; }
        public bool IsExpanded { get; set; }
        public List<SavedDatabaseModel> Databases { get; set; }

        public SavedAccountModel()
        {
            Databases = new List<SavedDatabaseModel>();
        }

        public SavedDatabaseModel GetDatabase(string dbName)
        {
            var db = Databases.FirstOrDefault(d => d.Name == dbName);
            if(db== null)
            {
                db = new SavedDatabaseModel { Name = dbName };
                Databases.Add(db);
            }
            return db;
        }

        public void RemoveDatabase(string dbName)
        {
            var db = Databases.FirstOrDefault(d => d.Name == dbName);
            if (db != null)
                Databases.Remove(db);
        }

        public void RemoveCollection(string dbName, string collName)
        {
            var db = Databases.FirstOrDefault(d => d.Name == dbName);
            if (db != null)
            {
                var cl = db.Collections.FirstOrDefault(c => c.Name == collName);
                if (cl != null)
                    db.Collections.Remove(cl);
            }
        }

        public SavedCollectionModel GetCollection(string dbName, string collName)
        {
            var db = this.GetDatabase(dbName);
            return db.GetCollection(collName);
        }
    }
}
