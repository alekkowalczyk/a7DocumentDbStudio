using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class AccountCredentialsModel
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }

        public AccountCredentialsModel()
        {

        }

        public AccountCredentialsModel(string endpoint, string key)
        {
            this.Endpoint = endpoint;
            this.Key = key;
        }
    }
}
