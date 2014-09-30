using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MockBonds
{
    public class Data
    {
        public string ISIN { get; private set; }
        public double Price { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Data()
        {
        }

        public Data(string isin, double price, DateTime timestamp)
        {
            ISIN = isin;
            Price = price;
            Timestamp = timestamp;
        }
    }
}
