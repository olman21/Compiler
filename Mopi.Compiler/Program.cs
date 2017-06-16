using Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LexicalAnalizer;
using Newtonsoft.Json;

namespace Mopi.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            LexicalAnalizer.Analizer helper = new LexicalAnalizer.Analizer();
            helper.LoadSourceCode(args[0]);
            var addedSymbols = helper.GetAddedErrors();
            string output = JsonConvert.SerializeObject(addedSymbols);

            Console.Write(output);
            Console.ReadLine();

        }
    }
}
