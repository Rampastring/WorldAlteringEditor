using Rampastring.Tools;
using System;
using System.Windows.Forms;
using TSMapEditor.Models;

namespace TSMapEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Test();
            return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void Test()
        {
            IniFile rulesIni = new IniFile("F:/Pelit/DTA Beta/INI/Rules.ini");
            IniFile firestormIni = new IniFile("F:/Pelit/DTA Beta/INI/Enhance.ini");
            Map map = new Map();
            map.Initialize(rulesIni, firestormIni);

            Console.WriteLine("Map loaded.");
        }
    }
}
