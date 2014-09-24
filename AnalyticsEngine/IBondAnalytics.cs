using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantLib;

namespace AnalyticsEngine
{
    public interface IBondAnalytics
    {
        double CalculateYield(string name, double price, Date settlementDate);
    }
}
