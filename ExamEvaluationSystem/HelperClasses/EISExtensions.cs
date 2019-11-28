using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public static class EISExtensions
    {
        public static string EncapsulateQuote(this string str)
        {
            return $"'{ str }'";
        }

        public static string Capitalize(this string str)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            var split = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var ret = "";
            foreach (var s in split)
                ret += char.ToUpper(s[0]) + s.Substring(1).ToLower() + " ";
            return ret.Trim();
        }
    }
}
