using BondDataApi.BondDataService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondDataApi
{
    class ProducerCallback : IProducerCallback
    {
        private Action<BondData> dataHandler;
        private BlockingCollection<BondData> queue;

        public ProducerCallback(Action<BondData> dataHandler)
        {
            this.dataHandler = dataHandler;
        }

        public ProducerCallback(BlockingCollection<BondData> queue)
        {
            this.queue = queue;
        }

        public void NewData(BondData data)
        {
            if (dataHandler != null)
                dataHandler(data);
            else if (queue != null)
                queue.Add(data);
        }
    }
}
