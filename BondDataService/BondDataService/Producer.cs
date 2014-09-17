using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BondDataService
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public class Producer : IProducer
    {
        //Use thread safe collections
        private ConcurrentDictionary<string, ConcurrentBag<ICallback>> subscribers = new ConcurrentDictionary<string, ConcurrentBag<ICallback>>();

        public void Subscribe(string name)
        {
            //If this is a new bond
            if (!subscribers.ContainsKey(name))
            {
                //Create a subscriber set for this bond
                if (!subscribers.TryAdd(name, new ConcurrentBag<ICallback>()))
                {
                    Console.WriteLine("Subscribe({0}): Failed to create subscriber bag", name);
                    return;
                }

                //Start generating prices asynchronously
                SendDataAsync(name);
            }

            //Add to the subscriber set if not already there
            var subscriber = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (subscribers[name].Contains(subscriber))
            {
                Console.WriteLine("Subscribe({0}): Already subscribed to {0}");
                return;
            }

            subscribers[name].Add(subscriber);
        }

        public void Unsubscribe(string name)
        {
            if (!subscribers.ContainsKey(name))
            {
                Console.WriteLine("Unsubscribe({0}): Not an active bond", name);
                return;
            }

            var subscriber = OperationContext.Current.GetCallbackChannel<ICallback>();
            if (!subscribers[name].Contains(subscriber))
            {
                Console.WriteLine("Unsubscribe({0}): Not being subcribed to", name);
                return;
            }

            ICallback callback;
            if (!subscribers[name].TryTake(out callback))
            {
                Console.WriteLine("Unsubscribe({0}): Failed to take subscriber from bag", name);
                return;
            }

            if (subscribers[name].IsEmpty)
            {
                ConcurrentBag<ICallback> bag;
                if (!subscribers.TryRemove(name, out bag))
                {
                    Console.WriteLine("Unsubscribe({0}): Failed to remove from dictionary", name);
                    return;
                }
            }
        }

        private async void SendDataAsync(string name)
        {
            var rand = new Random(OperationContext.Current.GetHashCode() - Environment.TickCount);

            await Task.Run(() => 
            {
                var basePrice = 100.0 + ((rand.NextDouble() - 0.5) * 10.0);

                while (subscribers.ContainsKey(name))
                {
                    var loopTime = Environment.TickCount;

                    //Calc a new price
                    basePrice += (rand.NextDouble() - 0.5) * 0.1;
                    var price = basePrice + Math.Sin(loopTime * 0.001);

                    var data = new BondData(name, price);
                    var bag = subscribers[name];

                    try
                    {
                        //Broadcast to subscribers
                        foreach (var callback in bag)
                            callback.NewData(data);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("{0} subscriber has timed out and will be removed", name);

                        //Remove the subscriber from the bag
                        ICallback callback;
                        if (!bag.TryTake(out callback))
                        {
                            Console.WriteLine("SendDataAsync({0}): Failed to remove subscriber from bag", name);
                            continue;
                        }

                        //Remove bond if no more subscribers
                        if (subscribers[name].IsEmpty)
                        {
                            if (!subscribers.TryRemove(name, out bag))
                            {
                                Console.WriteLine("SendDataAsync({0}): Failed to remove from dictionary", name);
                                continue;
                            }
                        }
                    }

                    Sync(loopTime);
                }
            });

            Console.WriteLine("{0} has no more subscribers, price generation has stopped", name);
        }

        private void Sync(int startTime)
        {
            var elapsed = Environment.TickCount - startTime;
            if (elapsed < 10)
                Thread.Sleep(10 - elapsed);
            else
                Thread.Yield();
        }
    }
}
