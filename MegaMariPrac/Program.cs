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
                try
                {
                    Process.Start("megamari.exe");
                }
                catch (Exception)
                {
                    MessageBox.Show("megamari.exe wasn't found, make sure MegaMariPrac.exe is in the same location as megamari.exe.", "Marisa not found");
                    Environment.Exit(1);
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
