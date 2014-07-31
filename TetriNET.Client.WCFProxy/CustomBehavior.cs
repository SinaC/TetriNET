using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

namespace TetriNET.Client.WCFProxy
{
    public class CustomBehavior : IClientMessageInspector, IEndpointBehavior
    {
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            string action = request.Headers.Action.Substring(request.Headers.Action.LastIndexOf('/')+1);
            string filename = String.Format("{0:HH-mm-ss-ffff}{1}.xml", DateTime.Now, action);
            string fullPathFilename = Path.Combine(@"D:\TEMP\TETRINETSOAPS", filename);
            //using (FileStream stream = new FileWriter(fullPathFilename, FileMode.Create))
            using (StreamWriter stream = new StreamWriter(fullPathFilename, false, Encoding.UTF8))
            {
                //MessageBuffer mb = request.CreateBufferedCopy(65536);
                //mb.WriteMessage(stream);
                //stream.Flush();
                stream.Write(request.ToString());
                stream.Flush();
            }
            return null;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            //
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            //
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            //
            clientRuntime.MessageInspectors.Add(this);
        }
    }
}
