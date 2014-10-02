using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondApp.Modules.PriceGrid.Models
{
    class Bond
    {
        public string ISIN { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
        public double YTM { get; set; }
        public double Duration { get; set; }
    }
}
