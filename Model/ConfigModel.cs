using a7DocumentDbStudio.Model.Config;
using a7ExtensionMethods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class ConfigModel
    {
        public List<SavedAccountModel> DocDbAccounts { get; set; }
        public string SelectedAccount { get; set; }
        public string SelectedDatabase { get; set; }
        public string SelectedCollection { get; set; }
        public string SelectedView { get; set; }
        public CollectionViewModel SelectedCollectionViewModel { get; set; }

        private string _filePath;

        private ConfigModel()
        {
            DocDbAccounts = new List<SavedAccountModel>();
        }

        private void setFilePath(string filePath)
        {
            this._filePath = filePath;
        }

        private void saveToFile()
        {
            if (_filePath.IsNotEmpty())
            {
                //File.WriteAllText(this._filePath, JObject.FromObject(this).ToString());
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(this, jsonSerializerSettings);
                File.WriteAllText(this._filePath, json);
            }
        }

        public SavedAccountModel GetAccount(string endpoint) => DocDbAccounts.FirstOrDefault(ac => ac.Credentials.Endpoint == endpoint);
        public SavedDatabaseModel getDatabase(string endpoint, string dbName) => GetAccount(endpoint)?.GetDatabase(dbName);
        public SavedCollectionModel GetCollection(string endpoint, string dbName, string collName) =>
            getDatabase(endpoint, dbName)?.Collections.FirstOrDefault(c => c.Name == collName);

        public void AddCollectionView(string endpoint, string dbName, CollectionViewModel view)
        {
            var account = GetAccount(endpoint);
            if(account != null)
            {
                var coll = account.GetCollection(dbName, view.CollectionName);
                coll.Views.Add(view);
                saveToFile();
            }
        }

        public void RemoveCollectionView(string endpoint, string dbName, CollectionViewModel view)
        {
            var account = GetAccount(endpoint);
            if (account != null)
            {
                var coll = account.GetCollection(dbName, view.CollectionName);
                var existingView = coll.Views.FirstOrDefault(v => v.ViewName == view.ViewName);
                if (existingView != null)
                {
                    coll.Views.Remove(existingView);
                    saveToFile();
                }
            }
        }

        public void SaveCollectionView(string endpoint, string dbName, CollectionViewModel view)
        {
            var account = GetAccount(endpoint);
            if (account != null)
            {
                var coll = account.GetCollection(dbName, view.CollectionName);
                var existingView = coll.Views.FirstOrDefault(v => v.ViewName == view.ViewName);
                if (existingView != null)
                {
                    var index = coll.Views.IndexOf(existingView);
                    coll.Views.Remove(existingView);
                    coll.Views.Insert(index, view);
                    saveToFile();
                }
            }
        }

        public void AddAccount(AccountCredentialsModel credentials)
        {
            this.DocDbAccounts.Add(new SavedAccountModel { Credentials = credentials });
            this.saveToFile();
        }

        public void ChangeAccountCredentials(string oldEndpoint, AccountCredentialsModel newCredentials)
        {
            var found = this.DocDbAccounts.FirstOrDefault(sam => sam.Credentials.Endpoint == oldEndpoint);
            if (found != null)
            {
                found.Credentials = newCredentials;
                saveToFile();
            }
        }

        public void RemoveAccount(string endpoint)
        {
            var found = this.DocDbAccounts.FirstOrDefault(sam => sam.Credentials.Endpoint == endpoint);
            if(found !=null)
            {
                this.DocDbAccounts.Remove(found);
                saveToFile();
            }
        }

        public void ExpandAccount(string endpoint)
        {
            var found = this.DocDbAccounts.FirstOrDefault(sam => sam.Credentials.Endpoint == endpoint);
            if (found != null)
            {
                found.IsExpanded = true;
                saveToFile();
            }
        }

        public void CollapseAccount(string endpoint)
        {
            var found = GetAccount(endpoint);
            if (found != null)
            {
                found.IsExpanded = false;
                saveToFile();
            }
        }

        public void ExpandDatabase(string dbName, string endpoint)
        {
            var found = getDatabase(endpoint, dbName);
            if (found != null)
            {
                found.IsExpanded = true;
                saveToFile();
            }
        }

        public void RemoveDatabase(string dbName, string endpoint)
        {
            var account = GetAccount(endpoint);
            if(account!=null)
            {
                account.RemoveDatabase(dbName);
            }
        }

        public void RemoveCollection(string dbName, string endpoint, string collName)
        {
            var account = GetAccount(endpoint);
            if (account != null)
            {
                account.RemoveCollection(dbName, collName);
            }
        }

        public void ExpandCollection(string dbName, string endpoint, string collection)
        {
            var found = GetCollection(endpoint, dbName, collection);
            if (found != null)
            {
                found.IsExpanded = true;
                saveToFile();
            }
        }

        public void CollapseCollection(string dbName, string endpoint, string collection)
        {
            var found = GetCollection(endpoint, dbName, collection);
            if (found != null)
            {
                found.IsExpanded = false;
                saveToFile();
            }
        }

        public void CollapseDatabase(string dbName, string endpoint)
        {
            var found = getDatabase(endpoint, dbName);
            if (found != null)
            {
                found.IsExpanded = false;
                saveToFile();
            }
        }

        public void SelectCollection(string name, string dbName, string endpoint, string viewName)
        {
            var foundAccount = GetAccount(endpoint);
            if(foundAccount != null)
            {
                foundAccount.IsExpanded = true;
                var db = foundAccount.GetDatabase(dbName);
                db.IsExpanded = true;
                this.SelectedAccount = endpoint;
                this.SelectedDatabase = dbName;
                this.SelectedCollection = name;
                this.SelectedView = viewName;
            }
            saveToFile();
        }

        public void UpdateSelectedCollectionView(Action<CollectionViewModel> action)
        {
            if (this.SelectedCollectionViewModel == null)
                this.SelectedCollectionViewModel = new CollectionViewModel();
            action(this.SelectedCollectionViewModel);
            saveToFile();
        }

        public void ResetSelectedCollection()
        {
            this.SelectedAccount = null;
            this.SelectedDatabase = null;
            this.SelectedCollection = null;
            saveToFile();
        }


        public static ConfigModel Get(string filePath)
        {
            if(File.Exists(filePath))
            {
                //var cm = JObject.Parse(File.ReadAllText(filePath)).ToObject<ConfigModel>();
                //cm.setFilePath(filePath);
                //return cm;
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var serialized = File.ReadAllText(filePath);
                var cm = JsonConvert.DeserializeObject<ConfigModel>(serialized, jsonSerializerSettings);
                cm.setFilePath(filePath);
                return cm;
            }
            else
            {
                var cm = new ConfigModel();
                cm.setFilePath(filePath);
                cm.saveToFile();
                return cm;
            }
        }
    }
}
