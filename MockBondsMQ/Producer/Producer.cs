using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MockBonds
{
    class Producer : IDisposable
    {
        private const string ExchangeName = "MockBonds";
        private const string RequestQueue = "Request";
        private const string DataQueue = "Data.";
        private const char Separator = '$';

        private IConnection connection;
        private IModel requestChannel;
        private ConcurrentDictionary<string, IModel> dataChannels = new ConcurrentDictionary<string, IModel>();
        private volatile bool run = true;

        public Producer()
        {
            //Connect to a rabbitmq server
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();

            //Process requests from clients
            ConsumeAsync(connection);
        }

        private async void ProduceAsync(IConnection connection, string isin)
        {
            await Task.Factory.StartNew(() =>
            {
                var rand = new Random(isin.GetHashCode() - Environment.TickCount);
                var basePrice = 100.0 + ((rand.NextDouble() - 0.5) * 10.0);

                //Create a channel for the consuming thread
                using (var channel = connection.CreateModel())
                {
                    dataChannels.TryAdd(isin, channel);

                    //Declare the exchange
                    channel.ExchangeDeclare(ExchangeName, "topic");

                    //Produce prices
                    while (run)
                    {
                        int loopStart = Environment.TickCount;

                        // Generate random price
                        basePrice += (rand.NextDouble() - 0.5) * 0.1;
                        var price = basePrice + Math.Sin(loopStart * 0.001);

                        var message = isin + Separator + price.ToString() + Separator + DateTime.Now;
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(ExchangeName, DataQueue + isin, null, body);

                        Sync(loopStart);
                    }

                    channel.Close();
                }
            },
            TaskCreationOptions.LongRunning);

            Console.WriteLine("Stopped producing data for {0}", isin);
        }

        private async void ConsumeAsync(IConnection connection)
        {
            await Task.Factory.StartNew(() =>
            {
                //Create a channel for the consuming thread
                using (requestChannel = connection.CreateModel())
                {
                    //Declare the exchange
                    requestChannel.ExchangeDeclare(ExchangeName, "topic");

                    //Decalre a temporary queue and bind it to the requests being sent to the exchange
                    var queueName = requestChannel.QueueDeclare().QueueName;
                    requestChannel.QueueBind(queueName, ExchangeName, RequestQueue);

                    //Create a consumer to get the request messages
                    var consumer = new QueueingBasicConsumer(requestChannel);
                    requestChannel.BasicConsume(queueName, true, consumer);
                    
                    //Consume message queue
                    while (run)
                    {
                        BasicDeliverEventArgs ea;
                        if (!consumer.Queue.Dequeue(1000, out ea))
                            continue;

                        var isin = Encoding.UTF8.GetString(ea.Body);

                        //Check ISIN does not already exist - produce prices for the requested ISIN
                        if (!dataChannels.ContainsKey(isin))
                            ProduceAsync(connection, isin);
                    }

                    requestChannel.Close();
                }
            },
            TaskCreationOptions.LongRunning);

            Console.WriteLine("Stopped consuming requests");
        }

        private void Sync(int startTime)
        {
            var elapsed = Environment.TickCount - startTime;
            if (elapsed < 10)
                Thread.Sleep(10 - elapsed);
            else
                Thread.Yield();
        }

        #region DISPOSE PATTERN
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                run = false;

                while (!requestChannel.IsClosed)
                    Thread.Sleep(1000);
                requestChannel.Dispose();

                foreach (var channel in dataChannels.Values)
                {
                    while (!channel.IsClosed)
                        Thread.Sleep(1000);
                    channel.Dispose();
                }

                connection.Dispose();
            }
        }

        ~Producer()
        {
            Dispose(false);
        }
        #endregion

        static void Main(string[] args)
        {
            using (var producer = new Producer())
            {
                Console.WriteLine("Press <Enter> to stop...");
                Console.ReadLine();
                Console.WriteLine("Closing...");
            }
        }
    }
}
