using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class DocumentModel : DocumentDbModelBase
    {
        private Document _document;
        private CollectionModel _sourceCollection;
        public string Id => _document.Id;
        public string SelfLink => _document.SelfLink;
        private JToken _json;
        public JToken Json
        {
            get
            {
                if(_json == null)
                    _json = toOrderedJToken();
                return _json;
            }
        }
        public string this[string key]
        {
            get
            {
                return Json.SelectToken(key)?.ToString();
            }
        }

        public DocumentModel(DocumentClient client, CollectionModel sourceCollection, Document document) : base(client)
        {
            _document = document;
            _sourceCollection = sourceCollection;
        }

        public async Task ReplaceFromString(string str)
        {
            var replaced = await _client.ReplaceDocumentAsync(_document.SelfLink, JObject.Parse(str));
            this._document = replaced.Resource;
            this._json = null;
        }

        public async Task ChangePropertyValue(string propertyPath, object newValue)
        {
            var js = this.Json;
            var token = js.SelectToken(propertyPath);
            if (token != null)
            {
                token.Replace(JToken.FromObject(newValue));
                var replaced = await _client.ReplaceDocumentAsync(_document.SelfLink, js);
                this._document = replaced.Resource;
                this._json = null;
            }
            else
            {
                throw new Exception($"'{propertyPath}' not found...");
            }
        }

        public override string ToString()
        {
            return Json.ToString().Replace(@"\n", "\n").Replace(@"\r", "\r");
        }

        /// <summary>
        /// gets the _* properties at the bottom
        /// </summary>
        /// <returns></returns>
        private JToken toOrderedJToken()
        {
            var ordered = new JObject();
            var jDocument = JToken.FromObject(_document);
            List<JToken> toBottom = new List<JToken>();
            foreach(var prop in jDocument.Children())
            {
                var jp = prop as JProperty;
                if (jp != null)
                {
                    if (jp.Name.StartsWith("_"))
                        toBottom.Add(jp);
                    else
                        ordered.Add(jp);
                }
                else
                {
                    ordered.Add(prop);
                }
            }
            foreach(var prop in toBottom)
            {
                ordered.Add(prop);
            }
            return ordered;
        }
    }
}
