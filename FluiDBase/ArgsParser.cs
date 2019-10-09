using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase
{
    public class ArgsParser
    {
        static readonly char[] spaceDelimeters = new[] { ' ', '\t' };
        private readonly char NameValueDelimiter;
        readonly char[] valueEndingSymbols;


        public ArgsParser(char nameValueDelimiter)
        {
            NameValueDelimiter = nameValueDelimiter;
            valueEndingSymbols = spaceDelimeters.Append(NameValueDelimiter).ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public List<KeyValuePair<string, string>> Parse(string s)
        {
            var rv = new List<KeyValuePair<string, string>>();

            int i = 0;
            do
            {
                i = GetNameValue(s, i, out string paramName, out string paramValue);
                if(paramName == "")
                    throw new ArgumentException("empty name for argument");
                else if (paramName != null)
                    rv.Add(new KeyValuePair<string, string>(paramName, paramValue));
            } while (i >= 0 && i <= s.Length - 1);

            return rv;
        }


        int GetNameValue(string s, int startIndex, out string name, out string value)
        {
            startIndex = SkipTrim(s, startIndex);
            if (startIndex < 0)
            {
                name = null;
                value = null;
                return startIndex;
            }

            int i = GetValue(s, startIndex, out name);
            if (i < 0)
            {
                value = null;
                return i;
            }

            i = SkipTrim(s, i);

            if (i < 0 || s[i] != NameValueDelimiter)
            {
                value = null;
                return i;
            }

            i = GetValue(s, i + 1, out value);
            return i;
        }


        /// <returns>the next index after value</returns>
        int GetValue(string s, int startIndex, out string value)
        {
            startIndex = SkipTrim(s, startIndex);
            if (startIndex < 0)
            {
                value = null;
                return startIndex;
            }

            if (s[startIndex] == '"')
            {
                int endIndex = FindUnescapedQuotes(s, startIndex + 1, '"');
                if (endIndex < 0)
                    throw new ArgumentException("quote is not closed");
                value = s.Substring(startIndex + 1, endIndex - startIndex - 1);
                value = Unescape(value, '"');
                return endIndex + 1;
            }
            else
            {
                int endIndex = s.IndexOfAny(valueEndingSymbols, startIndex);
                if (endIndex < 0)
                {
                    value = s.Substring(startIndex);
                    return -1;
                }
                else
                {
                    value = s.Substring(startIndex, endIndex - startIndex);
                    return endIndex;
                }
            }
        }


        int SkipTrim(string s, int startIndex)
        {
            return IndexOfAnyNot(s, startIndex, spaceDelimeters);
        }


        int IndexOfAnyNot(string s, int startIndex, char[] chars)
        {
            if (startIndex < 0 || startIndex >= s.Length)
                return -1;
            for (int i = startIndex; i <= s.Length - 1; i++)
                if (chars.All(c => c != s[i]))
                    return i;
            return -1;
        }


        int FindUnescapedQuotes(string s, int startIndex, char endingQuote)
        {
            int i = s.IndexOf(endingQuote, startIndex);
            if (i < 0)
                return i;
            if (s[i - 1] == '\\')
                return FindUnescapedQuotes(s, i + 1, endingQuote);
            return i;
        }


        string Unescape(string s, char c)
        {
            return s.Replace("\\" + c, c.ToString());
        }
    }
}
