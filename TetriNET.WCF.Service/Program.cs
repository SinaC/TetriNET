using System.ServiceProcess;

namespace TetriNET.WCF.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] servicesToRun = new ServiceBase[] 
                { 
                    new Service() 
                };
            ServiceBase.Run(servicesToRun);
        }
    }
}
