using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Enums
{
    public enum FilterFieldOperator
    {
        Equal, In, Like, NotEqual, GreaterThan, GreaterEqualThan, LessThan, LessEqualThan, Contains, StartsWith, EndsWith, IsNull, IsNotNull,
        Between
    }
}
