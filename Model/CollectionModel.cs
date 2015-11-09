using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using a7DocumentDbStudio.Utils;
using Newtonsoft.Json.Linq;

namespace a7DocumentDbStudio.Model
{
    public class CollectionModel : DocumentDbModelBase
    {
        private DocumentCollection _collection;
        private PoorMansTSqlFormatterLib.Interfaces.ISqlTokenizer _tokenizer;
        private PoorMansTSqlFormatterLib.Interfaces.ISqlTreeFormatter _formatter;
        private PoorMansTSqlFormatterLib.Interfaces.ISqlTokenParser _parser;
        public string Name => _collection.Id;
        public DatabaseModel Database { get; private set; }
        public string AccountEndpoint => Database.Account.Endpoint;
        public string DatabaseName => Database.Name;

        public CollectionModel(DocumentClient client, DatabaseModel database, DocumentCollection collection) : base(client)
        {
            Database = database;
            _collection = collection;
            _tokenizer = new PoorMansTSqlFormatterLib.Tokenizers.TSqlStandardTokenizer();
            _parser = new PoorMansTSqlFormatterLib.Parsers.TSqlStandardParser();
            _formatter = new PoorMansTSqlFormatterLib.Formatters.TSqlStandardFormatter(
                indentString: "\t",
                spacesPerTab: 4,
                maxLineWidth: 999,
                expandCommaLists: true,
                trailingCommas: false,
                spaceAfterExpandedComma: false,
                expandBooleanExpressions: true,
                expandCaseStatements: true,
                expandBetweenConditions: true,
                breakJoinOnSections: false,
                uppercaseKeywords: true,
                htmlColoring: false,
                keywordStandardization: false);
        }

        public async Task<DocumentModel> AddDocumentFromString(string json)
        {
            var added = await _client.CreateDocumentAsync(_collection.SelfLink, JObject.Parse(json));
            return new DocumentModel(this._client, this, added.Resource);
        }

        public async Task DeleteDocument(DocumentModel document)
        {
            await _client.DeleteDocumentAsync(document.SelfLink);
        }

        public async Task<DocumentListModel> GetDocuments(Filter.FilterExpressionData filter, int maxItems)
        {
            var whereBuilder = new WhereClauseBuilder(filter, _collection.Id);
            var sql = $"SELECT * FROM {_collection.Id} {whereBuilder.WhereClause}";
            return await GetDocuments(sql, whereBuilder.Parameters, maxItems);
        }

        public async Task<DocumentListModel> GetDocuments(string sql, Dictionary<string,object> parameters, int maxItems)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            var sqlParams = new SqlParameterCollection();
            foreach (var kv in parameters)
            {
                sqlParams.Add(new SqlParameter($"@{kv.Key}", kv.Value));
            }
            //sql = NSQLFormatter.Formatter.Format(sql);
            var tokenized = _tokenizer.TokenizeSQL(sql);
            var parsed = _parser.ParseSQL(tokenized);
            sql = _formatter.FormatSQLTree(parsed);
            var sqlQuery = new SqlQuerySpec(sql, sqlParams);
            var docs = await this._client.CreateDocumentQuery<Document>(_collection.SelfLink, sqlQuery, new FeedOptions() { MaxItemCount = maxItems, EnableScanInQuery = true }).QueryAsync(1); 
            return new DocumentListModel(
                docs.Select(d => new DocumentModel(this._client, this, d)).ToList(),
                sql,
                parameters);
        }

        public async Task DeleteCollectionFromDatabase()
        {
            await this._client.DeleteDocumentCollectionAsync(_collection.SelfLink);
        }
    }
}
