using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Symbols.Models;

namespace Symbols
{
    public class SymbolHelper
    {
        private Dictionary<string, Symbol> SymbolsTable;
        private Dictionary<string, Error> ErrosTable;
        public SymbolHelper() {
            string BaseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            using (var SymbolsJson = File.OpenText($@"{BaseDirectoryPath}\symbols.json"))
            {
                this.SymbolsTable = JsonConvert.DeserializeObject<Dictionary<string, Symbol>>(SymbolsJson.ReadToEnd());
            }
               
        }

        public Symbol GetToken(string token)
        {
            return SymbolsTable[token];
        }

        public bool TokenExists(string token)
        {
            return SymbolsTable.Any(s => s.Key == token);
        }

        public void SetToken(Symbol NewSymbol)
        {
            SymbolsTable.Add(NewSymbol.Key,NewSymbol);
        }

        public IEnumerable<Symbol> GetSymbolsByType(TokenType type) 
        {
            return this.SymbolsTable.Where(s => s.Value != null ? s.Value.type == type : false)
                        .Select(s=> {
                            s.Value.Key = s.Key;
                            return s.Value;
                        });
        }

        public Symbol GetDelimiter()
        {
            var Delimiter = this.SymbolsTable.FirstOrDefault(s => s.Value != null ? s.Value.type == TokenType.delimiter : false);
            if (Delimiter.Key == null)
                throw new ArgumentException("A delimiter token is required");

            Delimiter.Value.Id = Delimiter.Key;

            return Delimiter.Value;
        }
    }
}
