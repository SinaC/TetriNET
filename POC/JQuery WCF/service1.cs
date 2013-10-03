using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace POC.JQuery_WCF
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string GetData(int value);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        //[WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        string[] GetUser(string id);
    }

    public class User
    {

        Dictionary<int, string> users = null;
        public User()
        {
            users = new Dictionary<int, string>();
            users.Add(1, "pranay");
            users.Add(2, "Krunal");
            users.Add(3, "Aditya");
            users.Add(4, "Samir");
        }

        public string[] GetUser(int Id)
        {
            var user = from u in users
                       where u.Key == Id
                       select u.Value;

            return user.ToArray<string>();
        }

        public string[] GetAllUsers()
        {
            return users.Values.ToArray();
        }
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";


        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }


        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Service1 : IService1
    {
        private ServiceHost _serviceHost;

        public Service1()
        {
            _serviceHost = new ServiceHost(this);
            _serviceHost.Open();

            foreach (var endpt in _serviceHost.Description.Endpoints)
            {
                Console.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Console.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Console.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public string[] GetUser(string id)
        {
            //return new User().GetUser(Convert.ToInt32(id));
            return new User().GetAllUsers();
        }
    }
}
