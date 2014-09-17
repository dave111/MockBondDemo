using BondDataApi;
using BondDataApi.BondDataService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BondDataClient
{
    class Program
    {
        private BlockingCollection<BondData> queue = new BlockingCollection<BondData>();
        private volatile bool run = true;

        private void Process(BondData data)
        {
            Console.WriteLine("NewData({0}, {1}, {2})", data.Name, data.Price.ToString("000.00"), data.Timestamp.ToString("HH:mm:ss.FFF"));
        }

        private async void ConsumeQueueAsync()
        {
            await Task.Run(() =>
            {
                while (run)
                {
                    BondData data = queue.Take();
                    Console.WriteLine("NewData({0}, {1}, {2})", data.Name, data.Price.ToString("000.00"), data.Timestamp.ToString("HH:mm:ss.FFF"));
                    Thread.Yield();
                }
            });
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            var session = new Session(program.Process);
            //var session = new Session(program.queue);
            
            program.ConsumeQueueAsync();
            for (int i = 1; i < 11; ++i)
                session.Subscribe("Bond" + i);

            Console.Read();

            for (int i = 1; i < 11; ++i)
                session.Unsubscribe("Bond" + i);

            program.run = false;
        }
    }
}
