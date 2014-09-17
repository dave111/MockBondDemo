using BondDataApi.BondDataService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace BondDataApi
{
    public class Session
    {
        private const string uri = "http://localhost:8081/BondData";

        private InstanceContext context;
        private ConcurrentDictionary<string, ProducerClient> producers = new ConcurrentDictionary<string, ProducerClient>();

        public Session(Action<BondData> dataHandler)
        {
            context = new InstanceContext(new ProducerCallback(dataHandler));
        }

        public Session(BlockingCollection<BondData> queue)
        {
            context = new InstanceContext(new ProducerCallback(queue));
        }

        public bool Subscribe(string name)
        {
            if (producers.ContainsKey(name))
                return false;

            var client = new BondDataService.ProducerClient(context, new WSDualHttpBinding(), new EndpointAddress(uri));
            if (!producers.TryAdd(name, client))
                return false;

            Console.WriteLine("Subscribing to {0}...", name);
            producers[name].Subscribe(name);
            Console.WriteLine("Subscription finished");
            return true;
        }

        public bool Unsubscribe(string name)
        {
            if (!producers.ContainsKey(name))
                return false;

            producers[name].Unsubscribe(name);
            return true;
        }
    }
}
