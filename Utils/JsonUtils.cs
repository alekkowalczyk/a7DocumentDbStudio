using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    public class JsonUtils
    {
        public static bool Validate(string json)
        {
            try
            {
                JObject.Parse(json);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
