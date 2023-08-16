using System;
using System.IO;
using System.Reflection;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using Westwind.Scripting;

namespace TSMapEditor.Scripts
{
    public static class ScriptRunner
    {
        private static object scriptClassInstance;
        private static MethodInfo getDescriptionMethod;
        private static MethodInfo performMethod;
        private static MethodInfo getSuccessMessageMethod;

        public static string RunScript(Map map, string scriptPath)
        {
            if (scriptClassInstance == null || performMethod == null || getSuccessMessageMethod == null)
                throw new InvalidOperationException("Script not properly compiled!");

            try
            {
                performMethod.Invoke(scriptClassInstance, new object[] { map });
                return (string)getSuccessMessageMethod.Invoke(scriptClassInstance, null);
            }
            catch (Exception ex) // rare case where catching Exception is OK, we cannot know what the script can throw
            {
                return "An error occurred while running the script. Returned error message: " + Environment.NewLine + Environment.NewLine + ex.Message;
            }
        }

        public static string GetDescriptionFromScript(Map map, string scriptPath)
        {
            if (!File.Exists(scriptPath))
                return null;

            var sourceCode = File.ReadAllText(scriptPath);
            bool compileSuccess = CompileSource(map, sourceCode);
            if (!compileSuccess)
                return null;

            return (string)getDescriptionMethod.Invoke(scriptClassInstance, null);
        }

        private static bool CompileSource(Map map, string source)
        {
            var script = new CSharpScriptExecution() { SaveGeneratedCode = true };
            script.AddDefaultReferencesAndNamespaces();
            script.AddAssembly(typeof(System.Windows.Forms.MessageBox));
            script.AddAssembly(map.Units.GetType());
            script.AddAssembly(typeof(Map));
            script.AddAssembly(typeof(ITileImage));
            script.AddAssembly(typeof(Theater));
            script.AddAssembly(typeof(Point2D));
            script.AddAssembly(typeof(Constants));
            script.AddNamespace("TSMapEditor");
            script.AddNamespace("TSMapEditor.Models");
            script.AddNamespace("TSMapEditor.Rendering");
            script.AddNamespace("TSMapEditor.GameMath");

            getDescriptionMethod = null;
            performMethod = null;
            getSuccessMessageMethod = null;

            object instance = script.CompileClass(source);
            if (script.Error)
                return false;

            scriptClassInstance = instance;
            Type classType = instance.GetType();
            var methods = classType.GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.Name == "GetDescription")
                {
                    getDescriptionMethod = method;

                    if (getDescriptionMethod.ReturnType != typeof(string))
                        return false;
                }
                else if (method.Name == "Perform")
                {
                    performMethod = method;
                }
                else if (method.Name == "GetSuccessMessage")
                {
                    getSuccessMessageMethod = method;

                    if (getSuccessMessageMethod.ReturnType != typeof(string))
                        return false;
                }
            }

            return getDescriptionMethod != null && performMethod != null && getSuccessMessageMethod != null;
        }
    }
}
