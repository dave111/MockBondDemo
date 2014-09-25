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
        void Add(string isin, string issueDate, string maturityDate, double coupon);

        double Yield(string isin, double cleanPrice, string refDate = null);

        double NetPresentValue(string isin);

        double ModifiedDuration(string isin, double yield, string refDate = null);
    }
}
