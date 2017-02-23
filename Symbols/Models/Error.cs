using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbols.Models
{
    public class Error
    {
        public int Line { get; set; }
        public string Type { get; set; }
        public AnalizerType Analizer { get; set; }
        public string AnalizerName
        {
            get
            {
                return Analizer.ToString();
            }
        }
        public string Message { get; set; }
    }
}
