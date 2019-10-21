using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase
{
    public static class StringExtensions
    {
        public static string[] Concat(this string[] ar, string s)
        {
            if (ar == null || ar.Length == 0)
                if (string.IsNullOrEmpty(s))
                    return null;
                else
                    return new[] { s };
            else
                if (string.IsNullOrEmpty(s))
                return ar.ToArray();
            else
                return ar.Concat(new[] { s }).ToArray();
        }


        public static string TrimOrNullIfEmpty(this string s)
        {
            if (s == null) return null;
            s = s.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return s;
        }
    }
}
