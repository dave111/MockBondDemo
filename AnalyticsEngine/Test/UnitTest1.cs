using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalyticsEngine;

namespace AnalyticsTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestYield()
        {
            var analytics = new BondAnalytics();
            var yield = analytics.CalculateYield("Test", 104.0);
            Assert.AreEqual(0.04, yield, 0.001);
        }
    }
}
