using a7DocumentDbStudio.Utils;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class AccountModel : DocumentDbModelBase
    {
        public AccountCredentialsModel Credentials { get; private set; }
        public string Endpoint => Credentials.Endpoint;
        public List<DatabaseModel> Databases { get; private set; }

        public AccountModel(AccountCredentialsModel credentials) : base(new DocumentClient(new Uri(credentials.Endpoint), credentials.Key))
        {
            Credentials = credentials;
        }

        /// <summary>
        /// if Endpoint and/or Key changed in the Credentials value - with this function we refresh the connection
        /// </summary>
        /// <returns></returns>
        public async Task RefreshCredentials()
        {
            this._client = new DocumentClient(new Uri(Credentials.Endpoint), Credentials.Key);
            await this.RefreshDatabases();
        }

        public async Task RefreshDatabases()
        {
            this.Databases = new List<DatabaseModel>();
            FeedResponse<Database> databases;
            using (PerformanceWatcher.Start("ReadDatabaseFeed"))
            {
                databases = await this._client.ReadDatabaseFeedAsync();
            }

            foreach (Database db in databases)
            {
                DatabaseModel model = new DatabaseModel(_client, this, db);
                this.Databases.Add(model);
            }
        }

        public async Task<DatabaseModel> CreateDatabase(string name)
        {
            var newDb = await this._client.CreateDatabaseAsync(new Database() { Id = name });
            return new DatabaseModel(_client, this, newDb.Resource);
        }
    }
}
