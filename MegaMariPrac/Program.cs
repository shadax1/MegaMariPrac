using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace MegaMariPrac
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process[] pname = Process.GetProcessesByName("megamari");
            if (pname.Length == 0)
            {
                MessageBox.Show("The megamari.exe process wasn't found, make sure the game is open first.", "Marisa not found");
                Environment.Exit(1);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
