using Rampastring.Tools;
using System;
using System.Text;
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
            Logger.WriteToConsole = true;

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
            IniFile mapIni = new IniFile("F:/Pelit/DTA Beta/Maps/Default/a_buoyant_city.map");
            Map map = new Map();
            map.LoadExisting(rulesIni, firestormIni, mapIni);

            Console.WriteLine();
            Console.WriteLine("Drawing 'megamap':");
            StringBuilder sb = new StringBuilder();
            foreach (var row in map.Tiles)
            {
                sb.Clear();
                int drawnTileCount = 0;
                foreach (var tile in row)
                {
                    if (tile == null)
                        continue;

                    drawnTileCount++;

                    sb.Append(tile.TileIndex.ToString("D4") + " ");
                }

                if (sb.Length > 0)
                    Console.WriteLine(sb.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Map loaded.");
        }
    }
}
