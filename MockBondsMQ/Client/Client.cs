using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockBonds
{
    class Client : IClient
    {
        private const string ExchangeName = "MockBonds";
        private const string RequestQueue = "Request";
        private const string DataQueue = "Data.";
        private const char Separator = '$';

        private IConnection connection;
        private IModel channel;
        private string queueName;
        private QueueingBasicConsumer consumer;
        private HashSet<string> subscribed = new HashSet<string>();

        public Client()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            
            channel = connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, "topic");
            queueName = channel.QueueDeclare().QueueName;

            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
        }

        public void Subscribe(string isin)
        {
            if (subscribed.Contains(isin))
                return;

            //Send request to start price generation
            var body = Encoding.UTF8.GetBytes(isin);
            channel.BasicPublish(ExchangeName, RequestQueue, null, body);

            //Bind queue
            channel.QueueBind(queueName, ExchangeName, DataQueue + isin);
        }

        public void Unsubscribe(string isin)
        {
            if (subscribed.Contains(isin))
                channel.QueueUnbind(queueName, ExchangeName, DataQueue + isin, null);
        }

        public Data GetNext()
        {
            //Assume all messages are just prices for now
            BasicDeliverEventArgs ea;
            if (!consumer.Queue.Dequeue(1000, out ea))
                return new Data();

            var message = Encoding.UTF8.GetString(ea.Body);
            var tokens = message.Split(Separator);
            if (tokens.Length != 3)
                return new Data();

            return new Data(tokens[0], double.Parse(tokens[1]), DateTime.Parse(tokens[2]));
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
                channel.Dispose();
                connection.Dispose();
            }
        }

        ~Client()
        {
            Dispose(false);
        }
        #endregion
    }
}
