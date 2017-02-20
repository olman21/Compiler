using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Symbols;
using Symbols.Models;
using LexicalAnalizer.Models;

namespace LexicalAnalizer
{
    public class Analizer : IDisposable
    {
        private StreamReader FileToAnalize;
        private SymbolHelper symbols;
        private List<CodeLine> CodeLines;
        public Analizer()
        {
            symbols = new SymbolHelper();
            CodeLines = new List<CodeLine>();
        }
        public void LoadSourceCode(string path)
        {
            FileToAnalize = File.OpenText(path);
            Symbol Delimiter = symbols.GetDelimiter();
            var AssigmentSymbols = symbols.GetSymbolsByType(TokenType.assigment);
            List<string> AdditionalWordSpliter = new List<string> { " ", Delimiter.Id };
            if (AssigmentSymbols != null)
            {
                AdditionalWordSpliter.AddRange(AssigmentSymbols.Select(s => s.Id).ToList());
            }


            Symbol CurrentToken = null;

            string CurrentLine = FileToAnalize.ReadLine();
            while (CurrentLine != null)
            {
                string[] LineStatements = CurrentLine.Split(new string[] { Delimiter.Id }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string Statement in LineStatements)
                {
                    string[] Words = CurrentLine.Split(AdditionalWordSpliter.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Word in Words)
                    {
                        Symbol ExistingToken = symbols.GetToken(Word);
                        if (ExistingToken != null && ExistingToken.type != TokenType.identifier)
                        {
                            CurrentToken = ExistingToken;
                        }
                        else if (CurrentToken != null)
                        {
                            Symbol NewSymbol = null;
                            switch (CurrentToken.type)
                            {
                                case TokenType.primitive:
                                    NewSymbol = new Symbol
                                    {
                                        Id = $"{Word}-amb0",
                                        IsCustom = true,
                                        name = Word,
                                        DataType = CurrentToken,
                                        type = TokenType.identifier
                                    };
                                    CurrentToken = NewSymbol;
                                    break;
                                case TokenType.identifier:

                                    NewSymbol = new Symbol
                                    {
                                        Id = $"{CurrentToken.Id}_const",
                                        Value = Word,
                                        IsCustom = true,
                                        type = TokenType.value

                                    };
                                    CurrentToken = null;
                                    break;
                            }

                            if (NewSymbol != null)
                                symbols.SetToken(NewSymbol);
                        }
                    }
                }

                CurrentLine = FileToAnalize.ReadLine();
            }
        }

        public IEnumerable<Symbol> GetAddedSymbols() {
            return symbols.GetSymbols();
        }

        public void Dispose()
        {
            FileToAnalize.Dispose();
        }

    }
}
