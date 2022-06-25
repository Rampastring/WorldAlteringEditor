using System;
using System.Windows.Forms;
using TSMapEditor.Rendering;

namespace TSMapEditor
{
    static class Program
    {
        public static string[] args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Program.args = args;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Environment.CurrentDirectory = Application.StartupPath.Replace('\\', '/');
            new GameClass().Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            MessageBox.Show("The map editor failed to launch.\r\n\r\nReason: " + ex.Message + "\r\n\r\n Stack trace: " + ex.StackTrace);
        }

        public static void DisableExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }
    }
}
