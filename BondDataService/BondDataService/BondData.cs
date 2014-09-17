using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BondDataService
{
    [DataContract]
    public class BondData
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public double Price { get; private set; }

        [DataMember]
        public DateTime Timestamp { get; private set; }
        
        public BondData(string name, double price)
        {
            Name = name;
            Price = price;
            Timestamp = DateTime.Now;
        }
    }
}
