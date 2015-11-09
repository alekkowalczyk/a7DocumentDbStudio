using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using a7DocumentDbStudio.Utils;

namespace a7DocumentDbStudio.Model
{
    public class DatabaseModel : DocumentDbModelBase
    {
        private Database _db;

        public AccountModel Account { get; private set; }
        public string Name => _db.Id;
        public List<CollectionModel> Collections { get; private set; }

        public DatabaseModel(DocumentClient client, AccountModel account, Database db) : base(client)
        {
            _db = db;
            Account = account;
        }

        public async Task RefreshCollections()
        {
            this.Collections = new List<CollectionModel>();
            FeedResponse<DocumentCollection> collections;
            using (PerformanceWatcher.Start("ReadCollectionFeed"))
            {
                collections = await this._client.ReadDocumentCollectionFeedAsync(_db.SelfLink);
            }

            foreach (DocumentCollection collection in collections)
            {
                CollectionModel model = new CollectionModel(_client, this, collection);
                this.Collections.Add(model);
            }
        }

        public async Task<CollectionModel> CreateCollection(string name)
        {
            var newCll = await _client.CreateDocumentCollectionAsync(_db.SelfLink, new DocumentCollection { Id = name });
            return new CollectionModel(_client, this, newCll);
        }

        public async Task DeleteDabaseFormAccount()
        {
            await this._client.DeleteDatabaseAsync(_db.SelfLink);
        }
    }
}
