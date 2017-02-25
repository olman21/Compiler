using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Symbols;
using Symbols.Models;
using LexicalAnalizer.Models;
using System.Text.RegularExpressions;

namespace LexicalAnalizer
{
    public class Analizer : IDisposable
    {
        private StreamReader FileToAnalize;
        private SymbolHelper symbols;
        private List<CodeLine> CodeLines;
        private readonly Symbol Delimiter;
        private readonly IEnumerable<Symbol> OAgrouper;
        private readonly IEnumerable<Symbol> CAgrouper;
        private readonly IEnumerable<Symbol> AssigmentSymbols;
        private readonly Symbol SingleComment;
        private readonly Symbol OpenMultilineComment;
        private readonly Symbol CloseMultilineComment;
        private readonly List<string> AdditionalWordSpliter;
        private int Scope;
        public Analizer()
        {
            symbols = new SymbolHelper();

            Symbol Delimiter = symbols.GetDelimiter();
            AssigmentSymbols = symbols.GetSymbolsByType(TokenType.assigment);
            OAgrouper = symbols.GetSymbolsByType(TokenType.OpenGrouper);
            CAgrouper = symbols.GetSymbolsByType(TokenType.CloseGrouper);
            SingleComment = symbols.GetSymbolsByType(TokenType.SingleLineComment).FirstOrDefault();
            OpenMultilineComment = symbols.GetSymbolsByType(TokenType.OpenMultiLineComment).FirstOrDefault();
            CloseMultilineComment = symbols.GetSymbolsByType(TokenType.ClosenMultiLineComment).FirstOrDefault();
            AdditionalWordSpliter = new List<string> { " ", Delimiter.Id };
            Scope = 0;


            AdditionalWordSpliter.AddRange(OAgrouper.Select(s => s.Id).ToList());
            AdditionalWordSpliter.AddRange(CAgrouper.Select(s => s.Id).ToList());
            AdditionalWordSpliter.AddRange(AssigmentSymbols.Select(s => s.Id).ToList());
        }
        public void LoadSourceCode(string path)
        {
            FileToAnalize = File.OpenText(path);
            int LineNumber = 1;

            string CurrentLine = FileToAnalize.ReadLine();
            while (CurrentLine != null)
            {
                ProccessLine(CurrentLine, ref LineNumber);
                
                CurrentLine = FileToAnalize.ReadLine();
                LineNumber++;
            }
        }

        private void ProccessLine(string CurrentLine, ref int LineNumber)
        {
           

            string[] LineStatements = CurrentLine.SplitAndKeepSeparators(new string[] { Delimiter.Id }).ToArray();

            foreach (string Statement in LineStatements)
            {
                if (string.IsNullOrEmpty(Statement?.Trim()) || Statement.Trim().StartsWith(SingleComment.Id))
                {
                    LineNumber++;
                    continue;
                }
                    
                string[] Words = CurrentLine.Split(AdditionalWordSpliter.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string Word in Words)
                {
                    string CleanWord = SanitizeToken(Word);

                    Symbol ExistingToken = symbols.GetToken(CleanWord);

                    if (ExistingToken != null)
                    {
                        switch (ExistingToken.type)
                        {
                            case TokenType.primitive:
                                ProcessPrimitive(ExistingToken, Statement, LineNumber, Scope);
                                break;
                            case TokenType.conditional:
                            case TokenType.loop:
                                BlockStructure(ExistingToken, Statement, ref FileToAnalize, ref LineNumber, ref Scope);
                                break;
                        }

                        break;
                    }
                    else
                    {
                        symbols.AddError(new Error
                        {
                            Analizer = AnalizerType.lexical,
                            Line = LineNumber,
                            Message = $"Unexpected Token {CleanWord}"
                        });
                    }

                }


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

        private void ProcessPrimitive(Symbol Token, string Statement, int LineNumber, int Scope)
        {
            var RegexMatch = Token.Pattern.Match(Statement);
            string OperatorsPattern = symbols.GetSymbolSet(TokenType.arithmetic);
            string AssigmentPattern = symbols.GetSymbolSet(TokenType.assigment);

            var ArithmeticRegex = new Regex($"[{OperatorsPattern}]");
            var AssigmentRegex = new Regex($"[{AssigmentPattern}]");

            if (!RegexMatch.Success)
            {
                //Determina si es una expresion libre de contexto
                Match assigmentMatch = AssigmentRegex.Match(Statement);

                if (assigmentMatch.Success)
                {
                    Match arithmetichMatch = AssigmentRegex.Match(Statement);
                    if (arithmetichMatch.Success)
                        return;
                }


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
                    Id = $"{Identifier.Value}-scope-{Scope}",
                    IsCustom = true,
                    name = Identifier.Value,
                    DataType = Token,
                    type = TokenType.identifier,
                    Scope = Scope
                });
            }

            if (Value != null)
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
        private void BlockStructure(Symbol Token, string Statement, ref StreamReader Reader, ref int LineNumber, ref int Scope)
        {
            string OpenBlockDelimiter = symbols.GetSymbolSet(TokenType.OpenBlockDelimiter);
            string CloseBlockDelimiter = symbols.GetSymbolSet(TokenType.CloseBlockDelimiter);
            int OpenStatement = 0;
            int CloseStatement = 0;
            string CodeBlock = "";
            while (Statement != null)
            {
                CodeBlock += Statement;
                if (Statement.IndexOf(OpenBlockDelimiter) > 0)
                {
                    OpenStatement++;
                }
                else if (Statement.IndexOf(CloseBlockDelimiter) > 0)
                {
                    CloseStatement++;
                }

                if (OpenStatement != 0 && OpenStatement == CloseStatement)
                {
                    break;
                }
                


                Statement = Reader.ReadLine();
                LineNumber++;
            }

            var match = Token.Pattern.Match(CodeBlock);
            if (!match.Success)
            {
                symbols.AddError(new Error
                {
                    Analizer = AnalizerType.lexical,
                    Line = LineNumber,
                    Message = $"Error in {Token.Id} Sintax"
                });
            }
            else {
                string body = match.Groups["body"].Value;
                Scope++;
                if (!string.IsNullOrEmpty(body) && body.Trim() != string.Empty)
                {
                    var BlockLines = body.SplitAndKeepSeparators(new string[] { "\\n" });
                    foreach(string Line in BlockLines)
                    {
                        ProccessLine(Line, ref LineNumber);
                        LineNumber++;
                    }
                }
            }



        }
        

        public string SanitizeToken(string Token) {
            return Regex.Replace(Token, @"[/\t/\s/\n]",string.Empty);
        }

        public void Dispose()
        {
            FileToAnalize.Dispose();
        }

    }
}
