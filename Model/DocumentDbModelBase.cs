using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class DocumentDbModelBase
    {
        protected DocumentClient _client;

        public DocumentDbModelBase(DocumentClient client)
        {
            _client = client;
        }
    }
}
