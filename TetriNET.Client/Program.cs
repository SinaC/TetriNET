using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TetriNET.Client
{
    public class Program
    {
        public class Test : IXmlSerializable
        {
            public IPAddress Address { get; set; }
            public XmlSchema GetSchema()
            {
                throw new NotImplementedException();
            }

            public void ReadXml(XmlReader reader)
            {
                string addr = reader.GetAttribute("Address");
                if (!String.IsNullOrEmpty(addr))
                    Address = IPAddress.Parse(addr);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Address", Address.ToString());
            }
        }

        static void Main(string[] args)
        {
            Test test = new Test
            {
                Address = IPAddress.Parse("192.168.1.1")
            };

            XmlSerializer xs = new XmlSerializer(typeof(Test));
            using (StreamWriter wr = new StreamWriter(@"d:\temp\ipaddress.xml"))
            {
                xs.Serialize(wr, test);
            }

            Test test2;
            using (StreamReader sr = new StreamReader(@"d:\temp\ipaddress.xml"))
            {
                test2 = (Test)xs.Deserialize(sr);
            }

            //string baseAddress = "net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            //SimpleTetriNETProxyManager proxyManager = new SimpleTetriNETProxyManager(baseAddress);
            ExceptionFreeProxyManager proxyManager = new ExceptionFreeProxyManager(baseAddress);

            GameClient client = new GameClient(proxyManager);
            client.PlayerName = "Joel_" + Guid.NewGuid().ToString().Substring(0, 6);

            System.Console.WriteLine("Press any key to stop client");

            while (true)
            {
                client.Test();
                if (System.Console.KeyAvailable)
                    break;
                System.Threading.Thread.Sleep(250);
            }

            System.Console.ReadLine();
        }
    }
}
