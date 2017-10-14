using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTkConsole
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new MainWindow().Run(60);
        }
    }
}
