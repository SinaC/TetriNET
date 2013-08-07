using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();

            Console.WriteLine("Press enter to stop server");
            
            server.StartService();

            Console.ReadLine();
        }
    }
}
