using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LexicalAnalizer.Models
{
    public static class Extensions
    {
        public static IEnumerable<string> SplitAndKeepSeparators(this string source, string[] separators)
        {
            var builder = new StringBuilder();
            foreach (var cur in source)
            {
                builder.Append(cur);
                if (separators.Contains(cur.ToString()))
                {
                    yield return builder.ToString();
                    builder.Length = 0;
                }
            }
            if (builder.Length > 0)
            {
                yield return builder.ToString();
            }
        }

        public static IEnumerable<string> SplitIncludeSeparators(this Regex regex, string input)
        {
            var values = new List<string>();
            int pos = 0;
            foreach (Match m in regex.Matches(input))
            {
                values.Add(input.Substring(pos, m.Index - pos));
                values.Add(m.Value);
                pos = m.Index + m.Length;
            }
            values.Add(input.Substring(pos));

            return values;
        }
    }
}
