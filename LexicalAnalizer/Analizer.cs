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

            Delimiter = symbols.GetDelimiter();
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


            string[] LineStatements = CurrentLine.Trim().SplitAndKeepSeparators(new string[] { Delimiter.Id }).ToArray();

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
                                string CodeBlock = ExtractCodeBlock(Statement, ref LineNumber, ref FileToAnalize);
                                BlockStructure(ExistingToken, CodeBlock, ref LineNumber, ref Scope);
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
            string ParenthesisPattern = symbols.GetSymbolSet(TokenType.OpenGrouper) + symbols.GetSymbolSet(TokenType.CloseGrouper);
            string AssigmentPattern = symbols.GetSymbolSet(TokenType.assigment);

            var ArithmeticRegex = new Regex($"[{OperatorsPattern}{ParenthesisPattern}]");
            var AssigmentRegex = new Regex($"[{AssigmentPattern}]");

            if (!RegexMatch.Success)
            {
                //Determina si es una expresion libre de contexto
                Match assigmentMatch = ArithmeticRegex.Match(Statement);

                if (assigmentMatch.Success)
                {

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
                Symbol ScopeExistingToken = symbols.GetTokenInPreviousScope(Identifier.Value, Scope);
                if (ScopeExistingToken == null)
                {

                    semanticValidation(Identifier.Value, Value.Value, Token, LineNumber);

                    symbols.SetToken(new Symbol
                    {
                        Id = $"{Identifier.Value}-scope-{Scope}",
                        IsCustom = true,
                        name = Identifier.Value,
                        DataType = Token,
                        type = TokenType.identifier,
                        Scope = Scope
                    });


                    if (Value != null)
                    {
                        symbols.SetToken(new Symbol
                        {
                            Id = $"{Identifier.Value}-scope-{Scope}_const",
                            Value = Value.Value,
                            IsCustom = true,
                            type = TokenType.value
                        });
                    }
                }
                else
                {
                    symbols.AddError(new Error
                    {
                        Analizer = AnalizerType.lexical,
                        Line = LineNumber,
                        Message = $"The  identifier {Identifier.Value} was already defined before.",
                        Type = "Sintax"
                    });
                }

            }

        }

        private void semanticValidation(string identifier, string value, Symbol token, int LineNumber)
        {
            if (token.Id.Equals("int",StringComparison.CurrentCultureIgnoreCase))
            {
                int val;
                if (!int.TryParse(value, out val))
                {
                    symbols.AddError(new Error
                    {
                        Analizer = AnalizerType.semantic,
                        Line = LineNumber,
                        Message = $"The value {value} is not assignable to type {token.Id} in {identifier}"
                    });

                    
                }
                return;
            }

            if (token.Id.Equals("number", StringComparison.CurrentCultureIgnoreCase))
            {
                float val;
                if (!float.TryParse(value, out val))
                {
                    symbols.AddError(new Error
                    {
                        Analizer = AnalizerType.semantic,
                        Line = LineNumber,
                        Message = $"The value {value} is not assignable to type {token.Id} in {identifier}"
                    });

                   
                }

                return;
            }

            if (token.Id.Equals("bool", StringComparison.CurrentCultureIgnoreCase))
            {
                bool val;
                if (!bool.TryParse(value, out val))
                {
                    symbols.AddError(new Error
                    {
                        Analizer = AnalizerType.semantic,
                        Line = LineNumber,
                        Message = $"The value {value} is not assignable to type {token.Id} in {identifier}"
                    });
                    
                }

                return;
            }
        }

        private void BlockStructure(Symbol Token, string CodeBlock, ref int LineNumber, ref int Scope)
        {


            var match = Token.Pattern.Match(CodeBlock.ToString());
            if (!match.Success)
            {
                symbols.AddError(new Error
                {
                    Analizer = AnalizerType.lexical,
                    Line = LineNumber,
                    Message = $"Error in {Token.Id} Sintax"
                });
            }
            else
            {
                string body = match.Groups["body"].Value;
                Scope++;
                if (!string.IsNullOrEmpty(body) && body.Trim() != string.Empty)
                {
                    var BlockLines = body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    LineNumber -= BlockLines.Count();
                    foreach (string Line in BlockLines)
                    {
                        ProccessLine(Line, ref LineNumber);
                    }

                    Scope--;
                }
            }



        }

        private string ExtractCodeBlock(string Statement, ref int LineNumber, ref StreamReader Reader)
        {
            string OpenBlockDelimiter = symbols.GetSymbolSet(TokenType.OpenBlockDelimiter);
            string CloseBlockDelimiter = symbols.GetSymbolSet(TokenType.CloseBlockDelimiter);
            int OpenStatement = 0;
            int CloseStatement = 0;
            StringBuilder CodeBlock = new StringBuilder();
            while (Statement != null)
            {
                CodeBlock.AppendLine(Statement);
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
                if (Statement != null)
                    LineNumber++;
            }

            return CodeBlock.ToString();
        }


        public string SanitizeToken(string Token)
        {
            return Regex.Replace(Token, @"[/\t/\s/\n]", string.Empty);
        }

        //private void BuildSintaxTree(string Statement)
        //{
        //    string OperatorsPattern = symbols.GetSymbolSet(TokenType.arithmetic);
        //    string ParenthesisPattern = symbols.GetSymbolSet(TokenType.OpenGrouper) + symbols.GetSymbolSet(TokenType.CloseGrouper);
        //    string AssigmentPattern = symbols.GetSymbolSet(TokenType.assigment);

        //    var ArithmeticRegex = new Regex($"[{OperatorsPattern}{ParenthesisPattern}{AssigmentPattern}]");
        //    var AssigmentRegex = new Regex($"[{AssigmentPattern}]");
        //    Match arithmetichMatch = AssigmentRegex.Match(Statement);
        //    if (arithmetichMatch.Success)
        //    {
        //        var tokens = ArithmeticRegex.SplitIncludeSeparators(noSpacesStatement)?.ToList();
        //        List<SintaxTree> sintaxTree = new List<SintaxTree>();
        //        int startSintaxEvaluation = -1;
        //        for (int i = 0; i < tokens?.Count(); i++)
        //        {
        //            if (i <= startSintaxEvaluation)
        //                continue;
        //            var token = tokens[i];

        //            if (ArithmeticRegex.IsMatch(token))
        //            {

        //            }

        //            if (startSintaxEvaluation == -1 && AssigmentRegex.IsMatch(token))
        //            {
        //                startSintaxEvaluation = i;
        //            }
        //        }

        //    }
        //    var whiteSpaceRegex = new Regex(" *");
        //    string noSpacesStatement = whiteSpaceRegex.Replace(Statement, string.Empty);
        //    sintaxTree.Add(new SintaxTree
        //    {
        //        Id = token
        //    });

        //    Regex OpenParenthesisRegex = new Regex(symbols.GetSymbolSet(TokenType.OpenGrouper));
        //    Regex CloseParenthesisRegex = new Regex(symbols.GetSymbolSet(TokenType.CloseGrouper));

        //    if (OpenParenthesisRegex.IsMatch(rightSide))
        //    {
        //        for (int i = currentIndex; i < tokens.Count(); i++)
        //        {

        //        }
        //    }



        //} 

        public void Dispose()
        {
            FileToAnalize.Dispose();
        }

    }
}
