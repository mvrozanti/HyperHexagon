using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

//∮ 
namespace HyperHexagon {
    class Program {
        static void Main(string[] args) {// b r o k e n
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            SuperHexagon sh = new SuperHexagon();
            sh.StartAvoid(true);
        }
    }
}
