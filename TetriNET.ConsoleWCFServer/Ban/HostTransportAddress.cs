using System;
using System.Net;
using System.Runtime.Serialization;

namespace TetriNET.ConsoleWCFServer.Ban
{
    public abstract class HostTransportAddress : ISerializable
    {
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    [Serializable]
    public class IPTransportAddress : HostTransportAddress
    {
        public IPAddress Address { get; set; }

        protected IPTransportAddress(SerializationInfo info, StreamingContext context)
        {
            Address = IPAddress.Parse(info.GetString("IP"));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IP", Address.ToString());
        }
    }

    [Serializable]
    public class BuiltInAddress : HostTransportAddress
    {
        public string MachineName { get; set; }
        public string UserName { get; set; }

        protected BuiltInAddress(SerializationInfo info, StreamingContext context)
        {
            MachineName = info.GetString("MachineName");
            UserName = info.GetString("UserName");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MachineName", MachineName);
            info.AddValue("UserName", UserName);
        }
    }
}