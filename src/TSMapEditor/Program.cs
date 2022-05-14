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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Environment.CurrentDirectory = Application.StartupPath.Replace('\\', '/');
            new GameClass().Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            MessageBox.Show(ex.Message);
        }
    }
}
