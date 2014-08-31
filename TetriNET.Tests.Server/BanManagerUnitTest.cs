using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Server.BanManager;
using TetriNET.Server.Interfaces;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class BanManagerUnitTest
    {
        private IBanManager CreateBanManager()
        {
            return new BanManager();
        }

        [TestMethod]
        public void TestIsBannedFalseWhenNoBannedPlayers()
        {
            IBanManager banManager = CreateBanManager();

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsFalse(isBanned);
        }

        [TestMethod]
        public void TestIsBannedTrueWhenBannedPlayers()
        {
            IBanManager banManager = new BanManager();
            banManager.Ban("joel", IPAddress.Parse("127.0.0.1"), BanReasons.Spam);

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.0.0.1"));

            Assert.IsTrue(isBanned);
        }

        [TestMethod]
        public void TestIsBannedFalseOnUnknownAddress()
        {
            IBanManager banManager = new BanManager();
            banManager.Ban("joel", IPAddress.Parse("127.0.0.1"), BanReasons.Spam);

            bool isBanned = banManager.IsBanned(IPAddress.Parse("127.1.1.1"));

            Assert.IsFalse(isBanned);
        }
    }
}
