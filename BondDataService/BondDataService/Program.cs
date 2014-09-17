using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace BondDataService
{
    class Program
    {
        private const string uri = "http://localhost:8081/BondData";

        static void Main(string[] args)
        {
            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(Producer), new Uri(uri)))
            {
                var binding = new WSDualHttpBinding();
                binding.SendTimeout = TimeSpan.FromSeconds(10.0);
                host.AddServiceEndpoint(typeof(IProducer), binding, uri);

                // Enable metadata publishing.
                var smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                host.Open();
                
                Console.WriteLine("Press <Enter> to stop the service...");
                Console.ReadLine();
                Console.WriteLine("Closing...");

                host.Close();
            }
        }
    }
}
