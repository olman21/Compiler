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
            Analizer helper = new Analizer();
            helper.LoadSourceCode(@"C:\Olman\UAM\Modelos de programacion\sourceSample.mopi");
            var addedSymbols = helper.GetAddedErrors();
            string output = JsonConvert.SerializeObject(addedSymbols);

            Console.Write(output);
            Console.ReadLine();

        }
    }
}
