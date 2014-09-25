using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyticsEngine;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string refDate = "24/09/2014";
            var analytics = new BondAnalytics(refDate);

            var xDoc = XElement.Load("Bonds.xml");
            var xBonds = from xBond in xDoc.Descendants("Bond")
                         select xBond;

            var bonds = new List<Tuple<string, string, double>>();
            foreach (var xBond in xBonds)
            {
                bonds.Add(new Tuple<string, string, double>(xBond.Attribute("ISIN").Value, 
                                                            xBond.Element("Description").Value, 
                                                            double.Parse(xBond.Element("CleanPrice").Value)));

                analytics.Add(xBond.Attribute("ISIN").Value, 
                                xBond.Element("IssueDate").Value, 
                                xBond.Element("MaturityDate").Value, 
                                double.Parse(xBond.Element("Coupon").Value));
            }

            foreach (var tuple in bonds)
            {
                var yield = analytics.Yield(tuple.Item1, tuple.Item3);

                Console.WriteLine("ISIN:              {0}", tuple.Item1);
                Console.WriteLine("Description:       {0}", tuple.Item2);
                Console.WriteLine("Clean Price:       {0}", tuple.Item3.ToString(".00"));
                Console.WriteLine("NPV:               {0}", analytics.NetPresentValue(tuple.Item1).ToString(".000000"));
                Console.WriteLine("Yield %:           {0}", (yield * 100.0).ToString(".000000"));
                Console.WriteLine("Modified Duration: {0}", analytics.ModifiedDuration(tuple.Item1, yield).ToString(".00")); //2.89
                Console.WriteLine();
            }

            Console.Read();
        }
    }
}

