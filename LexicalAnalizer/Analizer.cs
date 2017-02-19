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
            int LineNumber = 0;
            string CurrentLine = FileToAnalize.ReadLine();
            while (CurrentLine != null)
            {
                CodeLines.Add(new CodeLine {
                    Line = LineNumber,
                    Content = CurrentLine
                });

                CurrentLine = FileToAnalize.ReadLine();
            }

            string FullText = CodeLines.Aggregate(new StringBuilder(),(sb,current)=> sb.Append(current.Content)).ToString();
            Symbol Delimiter = symbols.GetDelimiter();
            string[] AllSentences = FullText.Split(new string[]{ Delimiter.Id }, StringSplitOptions.RemoveEmptyEntries);

            foreach(string ActualLine in AllSentences)
            {
                string[] Words = ActualLine.Split(' ');
                foreach(string Word in Words)
                {
                    if (symbols.GetToken(Word) != null)
                    {

                    }
                    else {

                    }
                }
            }
        }

        public void Dispose()
        {
            FileToAnalize.Dispose();
        }

    }
}
