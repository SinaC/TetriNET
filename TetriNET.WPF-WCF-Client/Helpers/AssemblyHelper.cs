using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public class AssemblyHelper
    {
        public static Assembly GetEntryAssembly()
        {
            Assembly asm = null;

            try
            {
                if (DesignMode.IsInDesignModeStatic)
                    asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.EntryPoint != null && x.GetTypes().Any(t => t.IsSubclassOf(typeof(Application))));
                else
                    asm = Assembly.GetEntryAssembly();
            }
            catch
            {
                /* eat any exceptions here */
            }
            return asm ?? Assembly.GetExecutingAssembly();
        }
    }
}