using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class DocumentListModel
    {
        public List<DocumentModel> Documents { get; private set; }
        public string Sql { get; private set; }

        public DocumentListModel(List<DocumentModel> documents, string sql, Dictionary<string, object> parameters)
        {
            Documents = documents;
            this.Sql = sql;
            foreach(var kv in parameters)
            {
                if(kv.Value is string)
                    this.Sql = this.Sql.Replace($"@{kv.Key}", $"'{kv.Value?.ToString()}'");
                else
                    this.Sql = this.Sql.Replace($"@{kv.Key}", kv.Value?.ToString());
            }
        }
    }
}
