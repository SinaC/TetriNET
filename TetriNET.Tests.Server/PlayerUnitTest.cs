using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PlayerManager;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class PlayerUnitTest
    {
        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestNonNullName()
        {
            try
            {
                IPlayer player = new Player(0, null, new CountCallTetriNETCallback());

                Assert.Fail("ArgumentNullException on name not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "name");
            }
        }

        [TestMethod]
        public void TestNonNullCallback()
        {
            try
            {
                IPlayer player = new Player(0, "player1", null);

                Assert.Fail("ArgumentNullException on callback not raised");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(ex.ParamName, "callback");
            }
        }

        [TestMethod]
        public void TestLastActionToClientUpdate()
        {
            IPlayer player = new Player(0, "player1", new CountCallTetriNETCallback());
            DateTime lastActionToClient = player.LastActionToClient;

            Thread.Sleep(1);
            player.OnHeartbeatReceived();

            Assert.AreNotEqual(lastActionToClient, player.LastActionToClient);
        }

        [TestMethod]
        public void TestExceptionFreeAction()
        {
            IPlayer player = new Player(0, "player1", new RaiseExceptionTetriNETCallback());

            player.OnHeartbeatReceived();

            Assert.IsTrue(true, "No exception occured");
        }

        [TestMethod]
        public void TestConnectionLostCalledOnException()
        {
            bool called = false;
            IPlayer player = new Player(0, "player1", new RaiseExceptionTetriNETCallback());
            player.ConnectionLost += entity => called = true;

            player.OnHeartbeatReceived();

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestConnectionLostNotCalledOnNoException()
        {
            bool called = false;
            IPlayer player = new Player(0, "player1", new CountCallTetriNETCallback());
            player.ConnectionLost += entity => called = true;

            player.OnHeartbeatReceived();

            Assert.IsFalse(called);
        }

        [TestMethod]
        public void TestSetTimeout()
        {
            IPlayer player = new Player(0, "joel", new CountCallTetriNETCallback());
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            player.SetTimeout();

            Assert.AreNotEqual(lastActionFromClient, player.LastActionFromClient);
            Assert.AreEqual(player.TimeoutCount, 1);
        }

        [TestMethod]
        public void TestResetTimeout()
        {
            IPlayer player = new Player(0, "joel", new CountCallTetriNETCallback());
            player.SetTimeout();
            DateTime lastActionFromClient = DateTime.Now;

            Thread.Sleep(1);
            player.ResetTimeout();

            Assert.AreNotEqual(lastActionFromClient, player.LastActionFromClient);
            Assert.AreEqual(player.TimeoutCount, 0);
        }

        [TestMethod]
        public void TestLastActionToClientNoUpdatedOnException()
        {
            IPlayer player = new Player(0, "player1", new RaiseExceptionTetriNETCallback());
            DateTime lastActionToClient = player.LastActionToClient;

            Thread.Sleep(1);
            player.OnHeartbeatReceived();

            Assert.AreEqual(lastActionToClient, player.LastActionToClient);
        }
    }
}
