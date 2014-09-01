using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class ServerTest
    {
        private IServer CreateServer()
        {
            //return new TetriNET.Server.Server();
            return null; // TODO
        }

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
