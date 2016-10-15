using Accord.Controls;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Statistics.Kernels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

//∮ 
namespace HyperHexagon {

    class Program {

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

        static void Main(string[] args) {
            SuperHexagon.vam = new VAMemory("Super Hexagon");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            SuperHexagon.StartAvoid(true);
        }

        static void GoControl() {
            Process[] processes = Process.GetProcesses();
            foreach (Process p in processes) {
                if (p.ProcessName == "notepad") {
                    Console.WriteLine("!");
                    SetForegroundWindow(p.MainWindowHandle);
                    //DoSend("Nexor");
                    SendKeys.SendWait("{LEFT}");
                }
            }
        }

        static void DoSend(String s) {
            if (s.Length == 1) {
                SendKeys.SendWait('{' + s + "}");
            } else {
                DoSend(s.ElementAt(0) + "");
                DoSend(s.Remove(0, 1));
            }
        }
    }
}
