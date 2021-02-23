using Rampastring.Tools;
using System;
using System.Windows.Forms;
using TSMapEditor.Rendering;

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

            new GameClass().Run();
        }
    }
}
