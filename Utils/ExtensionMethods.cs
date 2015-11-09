using a7DocumentDbStudio.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    public static class ExtensionMethods
    {
        public static PropertyType ToPropertyType(this JTokenType jtype)
        {
            if (jtype == JTokenType.Integer)
                return PropertyType.Integer;
            else if (jtype == JTokenType.Float)
                return PropertyType.Float;
            else if (jtype == JTokenType.Boolean)
                return PropertyType.Bool;
            else if (jtype == JTokenType.Date)
                return PropertyType.DateTime;
            else
                return PropertyType.String;
        }
    }
}
