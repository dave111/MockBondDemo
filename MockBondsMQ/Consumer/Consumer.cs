using MockBonds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MockBonds
{
    class Consumer
    {
        static volatile bool run = true;

        static async void ConsoleReadAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                Console.Read();
                run = false;
            },
            TaskCreationOptions.LongRunning);
        }

        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            using (var client = ClientFactory.CreateClient())
            {
                for (int i = 1; i <= 10; ++i)
                    client.Subscribe("Bond" + i);

                ConsoleReadAsync();

                while (run)
                {
                    var data = client.GetNext();
                    Console.WriteLine("{0}\t{1}\t{2}", data.ISIN, data.Price.ToString(".00"), data.Timestamp.ToString("HH:mm:ss.FFF"));
                }

                for (int i = 1; i <= 10; ++i)
                    client.Unsubscribe("Bond" + i);

                Console.WriteLine("Closing...");
            }
        }
    }
}
