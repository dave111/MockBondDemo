using QuantLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondExample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                #region MARKET DATA

                var calendar = new TARGET();

                var settlementDate = new Date(18, Month.September, 2008);
                // must be a business day
                settlementDate = calendar.adjust(settlementDate);

                int fixingDays = 3;
                uint settlementDays = 3;

                var todaysDate = calendar.advance(settlementDate, -fixingDays, TimeUnit.Days);
                // nothing to do with Date::todaysDate
                Settings.instance().setEvaluationDate(todaysDate);

                Console.WriteLine("Today: {0} {1} {2} {3}", todaysDate.weekday(), todaysDate.dayOfMonth(), todaysDate.month(), todaysDate.year());
                Console.WriteLine("Settlement date: {0} {1} {2} {3}", settlementDate.weekday(), settlementDate.dayOfMonth(), settlementDate.month(), settlementDate.year());

                // Building of the bonds discounting yield curve

                #endregion

                #region RATE HELPERS

                // RateHelpers are built from the above quotes together with
                // other instrument dependant infos.  Quotes are passed in
                // relinkable handles which could be relinked to some other
                // data source later.

                // Common data

                // ZC rates for the short end
                double zc3mQuote = 0.0096;
                double zc6mQuote = 0.0145;
                double zc1yQuote = 0.0194;

                var zc3mRate = new SimpleQuote(zc3mQuote);
                var zc6mRate = new SimpleQuote(zc6mQuote);
                var zc1yRate = new SimpleQuote(zc1yQuote);

                var zcBondsDayCounter = new Actual365Fixed();

                var zc3m = new DepositRateHelper(new QuoteHandle(zc3mRate),
                    new Period(3, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    zcBondsDayCounter);

                var zc6m = new DepositRateHelper(new QuoteHandle(zc6mRate),
                    new Period(6, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    zcBondsDayCounter);

                var zc1y = new DepositRateHelper(new QuoteHandle(zc1yRate),
                    new Period(1, TimeUnit.Years),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    zcBondsDayCounter);

                // setup bonds
                double redemption = 100.0;

                const uint numberOfBonds = 5;

                var issueDates = new Date[] { new Date(15, Month.March, 2005),
                    new Date(15, Month.June, 2005),
                    new Date(30, Month.June, 2006),
                    new Date(15, Month.November, 2002),
                    new Date(15, Month.May, 1987) };

                var maturities = new Date[] { new Date(31, Month.August, 2010),
                    new Date(31, Month.August, 2011),
                    new Date(31, Month.August, 2013),
                    new Date(15, Month.August, 2018),
                    new Date(15, Month.May, 2038) };

                var couponRates = new double[] { 0.02375,
                        0.04625,
                        0.03125,
                        0.04000,
                        0.04500 };

                var marketQuotes = new double[] { 100.390625,
                        106.21875,
                        100.59375,
                        101.6875,
                        102.140625 };

                var quote = new QuoteVector((int)numberOfBonds);
                for (uint i = 0; i < numberOfBonds; i++)
                {
                    var cp = new SimpleQuote(marketQuotes[i]);
                    quote.Add(cp);
                }

                var quoteHandle = new RelinkableQuoteHandleVector((int)numberOfBonds);
                for (int i = 0; i < (int)numberOfBonds; i++)
                {
                    quoteHandle.Add(new RelinkableQuoteHandle());
                    quoteHandle[i].linkTo(quote[i]);
                }

                // Definition of the rate helpers
                var bondsHelpers = new RateHelperVector((int)numberOfBonds);
                for (int i = 0; i < (int)numberOfBonds; i++)
                {
                    var schedule = new Schedule(issueDates[i],
                        maturities[i],
                        new Period(Frequency.Semiannual),
                        new UnitedStates(UnitedStates.Market.GovernmentBond),
                        BusinessDayConvention.Unadjusted,
                        BusinessDayConvention.Unadjusted,
                        DateGeneration.Rule.Backward,
                        false);

                    var bondHelper = new FixedRateBondHelper(quoteHandle[i],
                            settlementDays,
                            100.0,
                            schedule,
                            new DoubleVector(1) { couponRates[i] },
                            new ActualActual(ActualActual.Convention.Bond),
                            BusinessDayConvention.Unadjusted,
                            redemption,
                            issueDates[i]);

                    bondsHelpers.Add(bondHelper);
                }

                #endregion

                #region CURVE BUILDING

                // Any DayCounter would be fine.
                // ActualActual::ISDA ensures that 30 years is 30.0
                var termStructureDayCounter = new ActualActual(ActualActual.Convention.ISDA);
                //double tolerance = 1.0e-15;

                // A depo-bond curve
                var bondInstruments = new RateHelperVector();

                // Adding the ZC bonds to the curve for the short end
                bondInstruments.Add(zc3m);
                bondInstruments.Add(zc6m);
                bondInstruments.Add(zc1y);

                // Adding the Fixed rate bonds to the curve for the long end
                for (int i = 0; i < (int)numberOfBonds; i++)
                    bondInstruments.Add(bondsHelpers[i]);

                var bondDiscountingTermStructure = new PiecewiseFlatForward(settlementDate,
                    bondInstruments,
                    termStructureDayCounter);

                // Building of the Libor forecasting curve
                // deposits
                double d1wQuote = 0.043375;
                double d1mQuote = 0.031875;
                double d3mQuote = 0.0320375;
                double d6mQuote = 0.03385;
                double d9mQuote = 0.0338125;
                double d1yQuote = 0.0335125;
                // swaps
                double s2yQuote = 0.0295;
                double s3yQuote = 0.0323;
                double s5yQuote = 0.0359;
                double s10yQuote = 0.0412;
                double s15yQuote = 0.0433;

                #endregion

                #region QUOTES

                // SimpleQuote stores a value which can be manually changed;
                // other Quote subclasses could read the value from a database
                // or some kind of data feed.

                // deposits
                var d1wRate = new SimpleQuote(d1wQuote);
                var d1mRate = new SimpleQuote(d1mQuote);
                var d3mRate = new SimpleQuote(d3mQuote);
                var d6mRate = new SimpleQuote(d6mQuote);
                var d9mRate = new SimpleQuote(d9mQuote);
                var d1yRate = new SimpleQuote(d1yQuote);
                // swaps
                var s2yRate = new SimpleQuote(s2yQuote);
                var s3yRate = new SimpleQuote(s3yQuote);
                var s5yRate = new SimpleQuote(s5yQuote);
                var s10yRate = new SimpleQuote(s10yQuote);
                var s15yRate = new SimpleQuote(s15yQuote);

                #endregion

                #region RATE HELPERS

                // RateHelpers are built from the above quotes together with
                // other instrument dependant infos.  Quotes are passed in
                // relinkable handles which could be relinked to some other
                // data source later.

                // deposits
                var depositDayCounter = new Actual360();

                var d1w = new DepositRateHelper(new QuoteHandle(d1wRate),
                    new Period(1, TimeUnit.Weeks),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                var d1m = new DepositRateHelper(new QuoteHandle(d1mRate),
                    new Period(1, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                var d3m = new DepositRateHelper(new QuoteHandle(d3mRate),
                    new Period(3, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                var d6m = new DepositRateHelper(new QuoteHandle(d6mRate),
                    new Period(6, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                var d9m = new DepositRateHelper(new QuoteHandle(d9mRate),
                    new Period(9, TimeUnit.Months),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                var d1y = new DepositRateHelper(new QuoteHandle(d1yRate),
                    new Period(1, TimeUnit.Years),
                    (uint)fixingDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    true,
                    depositDayCounter);

                // setup swaps
                var swFixedLegFrequency = Frequency.Annual;
                var swFixedLegConvention = BusinessDayConvention.Unadjusted;
                var swFixedLegDayCounter = new Thirty360(Thirty360.Convention.European);
                var swFloatingLegIndex = new Euribor6M();

                var forwardStart = new Period(1, TimeUnit.Days);

                var s2y = new SwapRateHelper(new QuoteHandle(s2yRate),
                    new Period(2, TimeUnit.Years),
                    calendar,
                    swFixedLegFrequency,
                    swFixedLegConvention,
                    swFixedLegDayCounter,
                    swFloatingLegIndex,
                    new QuoteHandle(),
                    forwardStart);

                var s3y = new SwapRateHelper(new QuoteHandle(s3yRate),
                    new Period(3, TimeUnit.Years),
                    calendar,
                    swFixedLegFrequency,
                    swFixedLegConvention,
                    swFixedLegDayCounter,
                    swFloatingLegIndex,
                    new QuoteHandle(),
                    forwardStart);

                var s5y = new SwapRateHelper(new QuoteHandle(s5yRate),
                    new Period(5, TimeUnit.Years),
                    calendar,
                    swFixedLegFrequency,
                    swFixedLegConvention,
                    swFixedLegDayCounter,
                    swFloatingLegIndex,
                    new QuoteHandle(),
                    forwardStart);

                var s10y = new SwapRateHelper(new QuoteHandle(s10yRate),
                    new Period(10, TimeUnit.Years),
                    calendar,
                    swFixedLegFrequency,
                    swFixedLegConvention,
                    swFixedLegDayCounter,
                    swFloatingLegIndex,
                    new QuoteHandle(),
                    forwardStart);

                var s15y = new SwapRateHelper(new QuoteHandle(s15yRate),
                    new Period(15, TimeUnit.Years),
                    calendar,
                    swFixedLegFrequency,
                    swFixedLegConvention,
                    swFixedLegDayCounter,
                    swFloatingLegIndex,
                    new QuoteHandle(),
                    forwardStart);

                #endregion

                #region CURVE BUILDING

                // Any DayCounter would be fine.
                // ActualActual::ISDA ensures that 30 years is 30.0

                // A depo-swap curve
                var depoSwapInstruments = new RateHelperVector();
                depoSwapInstruments.Add(d1w);
                depoSwapInstruments.Add(d1m);
                depoSwapInstruments.Add(d3m);
                depoSwapInstruments.Add(d6m);
                depoSwapInstruments.Add(d9m);
                depoSwapInstruments.Add(d1y);
                depoSwapInstruments.Add(s2y);
                depoSwapInstruments.Add(s3y);
                depoSwapInstruments.Add(s5y);
                depoSwapInstruments.Add(s10y);
                depoSwapInstruments.Add(s15y);

                var depoSwapTermStructure = new PiecewiseFlatForward(settlementDate,
                    depoSwapInstruments,
                    termStructureDayCounter);

                // Term structures that will be used for pricing:
                // the one used for discounting cash flows
                var discountingTermStructure = new RelinkableYieldTermStructureHandle();
                // the one used for forward rate forecasting
                //var forecastingTermStructure = new RelinkableYieldTermStructureHandle();

                #endregion

                #region BONDS TO BE PRICED

                // Common data
                double faceAmount = 100;

                // Pricing engine
                var bondEngine = new DiscountingBondEngine(discountingTermStructure);

                // Zero coupon bond
                var zeroCouponBond = new ZeroCouponBond(settlementDays,
                    new UnitedStates(UnitedStates.Market.GovernmentBond),
                    faceAmount,
                    new Date(15, Month.August, 2013),
                    BusinessDayConvention.Following,
                    116.92,
                    new Date(15, Month.August, 2003));

                zeroCouponBond.setPricingEngine(bondEngine);

                // Fixed 4.5% US Treasury Note
                var fixedBondSchedule = new Schedule(new Date(15, Month.May, 2007),
                    new Date(15, Month.May, 2017),
                    new Period(Frequency.Semiannual),
                    new UnitedStates(UnitedStates.Market.GovernmentBond),
                    BusinessDayConvention.Unadjusted,
                    BusinessDayConvention.Unadjusted,
                    DateGeneration.Rule.Backward,
                    false);

                var fixedRateBond = new FixedRateBond((int)settlementDays,
                    faceAmount,
                    fixedBondSchedule,
                    new DoubleVector(1) { 0.045 },
                    new ActualActual(ActualActual.Convention.Bond),
                    BusinessDayConvention.ModifiedFollowing,
                    100.0,
                    new Date(15, Month.May, 2007));

                

                fixedRateBond.setPricingEngine(bondEngine);

                // Floating rate bond (3M USD Libor + 0.1%)
                // Should and will be priced on another curve later...

                var liborTermStructure = new RelinkableYieldTermStructureHandle();
                var libor3m = new USDLibor(new Period(3, TimeUnit.Months),
                    liborTermStructure);
                libor3m.addFixing(new Date(17, Month.July, 2008), 0.0278625);

                var floatingBondSchedule = new Schedule(new Date(21, Month.October, 2005),
                    new Date(21, Month.October, 2010),
                    new Period(Frequency.Quarterly),
                    new UnitedStates(UnitedStates.Market.NYSE),
                    BusinessDayConvention.Unadjusted,
                    BusinessDayConvention.Unadjusted,
                    DateGeneration.Rule.Backward,
                    true);

                var floatingRateBond = new FloatingRateBond(settlementDays,
                    faceAmount,
                    floatingBondSchedule,
                    libor3m,
                    new Actual360(),
                    BusinessDayConvention.ModifiedFollowing,
                    2,
                    // Gearings
                    new DoubleVector(1) { 1.0 },
                    // Spreads
                    new DoubleVector(1) { 0.001 },
                    // Caps
                    new DoubleVector(),
                    // Floors
                    new DoubleVector(),
                    // Fixing in arrears
                    true,
                    100.0,
                    new Date(21, Month.October, 2005));

                floatingRateBond.setPricingEngine(bondEngine);

                // Coupon pricers
                var pricer = new BlackIborCouponPricer();

                // optionLet volatilities
                double volatility = 0.0;
                var vol = new OptionletVolatilityStructureHandle(new ConstantOptionletVolatility(settlementDays,
                    calendar,
                    BusinessDayConvention.ModifiedFollowing,
                    volatility,
                    new Actual365Fixed()));

                pricer.setCapletVolatility(vol);
                NQuantLibc.setCouponPricer(floatingRateBond.cashflows(), pricer);

                // Yield curve bootstrapping
                //forecastingTermStructure.linkTo(depoSwapTermStructure);
                discountingTermStructure.linkTo(bondDiscountingTermStructure);

                // We are using the depo & swap curve to estimate the future Libor rates
                liborTermStructure.linkTo(depoSwapTermStructure);

                #endregion

                #region BOND PRICING

                Console.WriteLine();

                // write column headings
                int[] widths = new int[] { 0, 28, 38, 48 };

                Console.CursorLeft = widths[0]; Console.Write("                 ");
                Console.CursorLeft = widths[1]; Console.Write("ZC");
                Console.CursorLeft = widths[2]; Console.Write("Fixed");
                Console.CursorLeft = widths[3]; Console.WriteLine("Floating");

                //string separator = " | ";
                int width = widths[3];
                string rule = new string('-', width);
                string dblrule = new string('=', width);
                string tab = new string(' ', 8);

                Console.WriteLine(rule);

                Console.CursorLeft = widths[0]; Console.Write("Net present value");
                Console.CursorLeft = widths[1]; Console.Write(zeroCouponBond.NPV().ToString("000.00"));
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.NPV().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.NPV().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Clean price");
                Console.CursorLeft = widths[1]; Console.Write(zeroCouponBond.cleanPrice().ToString("000.00"));
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.cleanPrice().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.cleanPrice().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Dirty price");
                Console.CursorLeft = widths[1]; Console.Write(zeroCouponBond.dirtyPrice().ToString("000.00"));
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.dirtyPrice().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.dirtyPrice().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Accrued coupon");
                Console.CursorLeft = widths[1]; Console.Write(zeroCouponBond.accruedAmount().ToString("000.00"));
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.accruedAmount().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.accruedAmount().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Previous coupon");
                Console.CursorLeft = widths[1]; Console.Write("N/A");
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.previousCouponRate().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.previousCouponRate().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Next coupon");
                Console.CursorLeft = widths[1]; Console.Write("N/A");
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.nextCouponRate().ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.nextCouponRate().ToString("000.00"));

                Console.CursorLeft = widths[0]; Console.Write("Yield");
                Console.CursorLeft = widths[1]; Console.Write(zeroCouponBond.yield(new Actual360(), Compounding.Compounded, Frequency.Annual).ToString("000.00"));
                Console.CursorLeft = widths[2]; Console.Write(fixedRateBond.yield(new Actual360(), Compounding.Compounded, Frequency.Annual).ToString("000.00"));
                Console.CursorLeft = widths[3]; Console.WriteLine(floatingRateBond.yield(new Actual360(), Compounding.Compounded, Frequency.Annual).ToString("000.00"));

                double yield = fixedRateBond.yield(new Actual360(), Compounding.Compounded, Frequency.Annual);
                Console.CursorLeft = widths[2]; Console.Write(BondFunctions.duration(fixedRateBond, new InterestRate(yield, fixedRateBond.dayCounter(), Compounding.Compounded, Frequency.Annual), Duration.Type.Modified));
                
                Console.WriteLine();

                // Other computations
                Console.WriteLine("Sample indirect computations (for the floating rate bond): ");
                Console.WriteLine(rule);

                Console.WriteLine("Yield to Clean Price: {0}", floatingRateBond.cleanPrice(floatingRateBond.yield(new Actual360(), Compounding.Compounded, Frequency.Annual), new Actual360(), Compounding.Compounded, Frequency.Annual, settlementDate).ToString("000.00"));

                Console.WriteLine("Clean Price to Yield: {0}", floatingRateBond.yield(floatingRateBond.cleanPrice(), new Actual360(), Compounding.Compounded, Frequency.Annual, settlementDate).ToString("000.00"));

                /* "Yield to Price"
                "Price to Yield" */

                double milliseconds = timer.ElapsedMilliseconds;
                Console.WriteLine();
                Console.WriteLine("Run completed in " + milliseconds + "ms");

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.Read();
            }
        }
    }
}
