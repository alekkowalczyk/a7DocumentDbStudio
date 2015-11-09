using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    public static class StringUtils
    {
        public static string FirstCharacterToLower(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
                return str;

            return Char.ToLowerInvariant(str[0]).ToString() + str.Substring(1);
        }

        public static string FirstCharacterToUpper(string str)
        {
            if (String.IsNullOrEmpty(str) || Char.IsUpper(str, 0))
                return str;

            return Char.ToUpperInvariant(str[0]).ToString() + str.Substring(1);
        }

        public static string RemoveDoubleParenthesis(string initialString)
        {
            char[] s = new char[initialString.Length];
            char toRemove = '$';
            Stack<int> stack = new Stack<int>();

            for (int i = 0; i < s.Length; i++)
            {
                s[i] = initialString[i];
                if (s[i] == '(')
                    stack.Push(i);
                else if (s[i] == ')')
                {
                    int start = stack.Pop();
                    if ((start == 0 && i == (s.Length - 1))
                     || (s[start - 1] == '(' && s[i + 1] == ')'))
                    {
                        s[start] = s[i] = toRemove;
                    }
                }
            }

            return new string((from c in s where c != toRemove select c).ToArray());
        }
    }
}
