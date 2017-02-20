using Newtonsoft.Json;
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
        public string typeName
        {
            get
            {
                return type.ToString();
            }
        }
        public string name { get; set; }
        public string Id { get; set; }
        public string description { get; set; }
        public BitRange bitInterval { get; set; }
        [JsonIgnore]
        public string regex { get; set; }
        [JsonIgnore]
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

        public bool IsCustom { get; set; }
        public Symbol DataType { get; set; }
        public string Value { get; set; }
    }

    public struct BitRange
    {
        public int min { get; set; }
        public int max { get; set; }
    }
}
