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
            int LineNumber = 1;
            while (CurrentLine != null)
            {
                string[] LineStatements = CurrentLine.SplitAndKeepSeparators(new string[] { Delimiter.Id }).ToArray();

                foreach (string Statement in LineStatements)
                {
                    string[] Words = CurrentLine.Split(AdditionalWordSpliter.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Word in Words)
                    {
                        Symbol ExistingToken = symbols.GetToken(Word);

                        if (ExistingToken != null)
                        {
                            switch (ExistingToken.type)
                            {
                                case TokenType.primitive:
                                    ProcessPrimitive(ExistingToken, Statement, LineNumber);
                                    break;
                            }

                            break;
                        }
                        else
                        {
                            symbols.AddError(new Error {
                                Analizer = AnalizerType.lexical,
                                Line = LineNumber,
                                Message = $"Unexpected Token {Word}"
                            });
                        }

                    }
                }

                CurrentLine = FileToAnalize.ReadLine();
                LineNumber++;
            }
        }

        public IEnumerable<Symbol> GetAddedSymbols()
        {
            return symbols.GetSymbols();
        }

        public IEnumerable<Error> GetAddedErrors()
        {
            return symbols.GetErrors();
        }

        private void ProcessPrimitive(Symbol Token, string Statement, int LineNumber)
        {
            var RegexMatch = Token.Pattern.Match(Statement);
            if (!RegexMatch.Success)
            {
                symbols.AddError(new Error
                {
                    Analizer = AnalizerType.lexical,
                    Line = LineNumber,
                    Message = $"Invalid sintax for {Token.Id}",
                    Type = "Sintax"
                });

                return;
            }

            var Identifier = RegexMatch.Groups[TokenType.identifier.ToString()];
            var Value = RegexMatch.Groups[TokenType.value.ToString()];
            if (Identifier != null)
            {
                symbols.SetToken(new Symbol
                {
                    Id = $"{Identifier.Value}-amb0",
                    IsCustom = true,
                    name = Identifier.Value,
                    DataType = Token,
                    type = TokenType.identifier
                });
            }

            if(Value != null)
            {
                symbols.SetToken(new Symbol
                {
                    Id = $"{Identifier.Value}_const",
                    Value = Value.Value,
                    IsCustom = true,
                    type = TokenType.value
                });
            }
        }
        private void ConditionalStructure()
        {

        }

        private void LoopStructure()
        {

        }

        public void Dispose()
        {
            FileToAnalize.Dispose();
        }

    }
}
