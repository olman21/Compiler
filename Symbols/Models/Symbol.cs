using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Symbols.Models
{
    public class Symbol
    {
        private Regex _pattern;
        public TokenType type { get; set; }
        public string name { get; set; }
        public string Id { get; set; }
        public string description { get; set; }
        public BitRange bitInterval { get; set; }
        public string regex { get; set; }
        public Regex Pattern
        {
            get
            {
                if (_pattern != null)
                    return _pattern;
                if (regex == null)
                    return null;

                _pattern = new Regex(regex);
                return _pattern;
            }
        }

    }

    public struct BitRange
    {
        public int min { get; set; }
        public int max { get; set; }
    }
}
