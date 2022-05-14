using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MapEditorLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.WriteLine("DTA Scenario Editor Launcher");
            Console.WriteLine("By Rampastring");
            Console.WriteLine("http://rampastring.net");
            Console.WriteLine();

            new UpdaterLink().Run();

            Console.WriteLine("Launching scenario editor.");
            Process.Start(new ProcessStartInfo("DTAScenarioEditor.exe") { UseShellExecute = false });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string exceptLogPath = Environment.CurrentDirectory + "/LauncherCrash.log";
            Exception ex = (Exception)e.ExceptionObject;
            File.Delete(exceptLogPath);

            StringBuilder sb = new StringBuilder();

            Console.WriteLine("Unhandled exception! @ " + DateTime.Now.ToLongTimeString(), sb, exceptLogPath);
            LogLineGenerate("Message: " + ex.Message, sb, exceptLogPath);
            LogLineGenerate("Stack trace: " + ex.StackTrace, sb, exceptLogPath);

            if (ex.InnerException != null)
            {
                LogLineGenerate("***************************", sb, exceptLogPath);
                LogLineGenerate("InnerException information:", sb, exceptLogPath);
                LogLineGenerate("Message: " + ex.InnerException.Message, sb, exceptLogPath);
                LogLineGenerate("Stack trace: " + ex.InnerException.StackTrace, sb, exceptLogPath);
            }

            Console.WriteLine("An error was encountered while starting or updating the map editor.");
            Console.WriteLine("If the problem persists, please contact the developers for support.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

            Environment.Exit(255);
        }

        private static void LogLineGenerate(string text, StringBuilder sb, string exceptLogPath)
        {
            sb.Append(text + Environment.NewLine);
            Console.WriteLine(text + Environment.NewLine);
        }
    }
}
