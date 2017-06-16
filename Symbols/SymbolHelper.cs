using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Symbols.Models;
using System.Text.RegularExpressions;

namespace Symbols
{
    public class SymbolHelper
    {
        private Dictionary<string, Symbol> SymbolsTable;
        private List<Error> ErrorTable;
        public SymbolHelper() {
            string BaseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            using (var SymbolsJson = File.OpenText($@"{BaseDirectoryPath}\symbols.json"))
            {
                this.SymbolsTable = JsonConvert.DeserializeObject<Dictionary<string, Symbol>>(SymbolsJson.ReadToEnd());
            }

            ErrorTable = new List<Error>();
               
        }

        public Symbol GetToken(string token)
        {
            Symbol FoundToken;
            if(SymbolsTable.TryGetValue(token, out FoundToken))
                return FoundToken;
            return null;
        }

        public IEnumerable<Symbol> GetSymbols() {
            return SymbolsTable.Where(s => s.Value.IsCustom).Select(s=>s.Value);
        }

        public bool TokenExists(string token)
        {
            return SymbolsTable.Any(s => s.Key == token);
        }

        public void SetToken(Symbol NewSymbol)
        {
            SymbolsTable.Add(NewSymbol.Id,NewSymbol);
        }

        public IEnumerable<Symbol> GetSymbolsByType(TokenType type) 
        {
            return this.SymbolsTable.Where(s => s.Value != null && s.Value.type == type)
                        .Select(s=> s.Value);
        }

        public Symbol GetDelimiter()
        {
            var Delimiter = this.SymbolsTable.FirstOrDefault(s => s.Value != null ? s.Value.type == TokenType.delimiter : false);
            if (Delimiter.Key == null)
                throw new ArgumentException("A delimiter token is required");

            Delimiter.Value.Id = Delimiter.Key;

            return Delimiter.Value;
        }

        public void AddError(Error error)
        {
            ErrorTable.Add(error);
        }

        public List<Error> GetErrors()
        {
            return ErrorTable;
        }

        public string GetSymbolSet(TokenType type)
        {
            return string.Join("", GetSymbolsByType(type).Select(o => o.escaped??o.Id).ToArray());
        }

        public Symbol GetTokenInPreviousScope(string Identifier, int ActualScope) {
            return SymbolsTable.FirstOrDefault(s => s.Value?.name == Identifier && s.Value?.Scope <= ActualScope).Value;
        }
    }
}
