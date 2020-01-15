using CTrader.WebAPI;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTraderUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Task.Run(() => Startup.Start());
            Application.Run(new FormMain());
        }
    }
}
