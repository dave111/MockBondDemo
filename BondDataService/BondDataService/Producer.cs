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
        private ConcurrentDictionary<string, List<ICallback>> subscribers = new ConcurrentDictionary<string, List<ICallback>>();

        public void Subscribe(string name)
        {
            //If this is a new bond
            if (!subscribers.ContainsKey(name))
            {
                //Create a subscriber set for this bond
                if (!subscribers.TryAdd(name, new List<ICallback>()))
                {
                    Console.WriteLine("Subscribe({0}): Failed to create subscriber list", name);
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
            if (!subscribers[name].Remove(subscriber))
            {
                Console.WriteLine("Unsubscribe({0}): Failed to remove subscriber from list", name);
                return;
            }

            if (subscribers[name].Count == 0)
            {
                List<ICallback> dummy;
                if (!subscribers.TryRemove(name, out dummy))
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
                    ICallback current = null;
                    try
                    {
                        //Broadcast to subscribers
                        foreach (var callback in subscribers[name])
                        {
                            current = callback;
                            callback.NewData(data);
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("{0} subscriber has timed out and will be removed", name);

                        //Remove the subscriber from the bag
                        if (!subscribers[name].Remove(current))
                        {
                            Console.WriteLine("SendDataAsync({0}): Failed to remove subscriber from bag", name);
                            continue;
                        }

                        //Remove bond if no more subscribers
                        if (subscribers[name].Count == 0)
                        {
                            List<ICallback> dummy;
                            if (!subscribers.TryRemove(name, out dummy))
                            {
                                Console.WriteLine("SendDataAsync({0}): Failed to remove from dictionary", name);
                                continue;
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("SendDataAsync({0}): Subscriber list changed while processing", name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(name);
                        Console.WriteLine(e.GetType());
                        Console.WriteLine(e.Message);
                    }

                    Sync(loopTime);
                }
            });

            Console.WriteLine("SendDataAsync({0}): No more subscribers, price generation has stopped", name);
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
