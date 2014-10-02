using BondApp.Modules.PriceGrid.Models;
using BondApp.Modules.PriceGrid.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondApp.Modules.PriceGrid.ViewModels
{
    class PriceGridViewModel : IPriceGridViewModel
    {
        public IPriceGridView View { get; set; }
        public Bonds Bonds { get; set; }

        public PriceGridViewModel(IPriceGridView view)
        {
            View = view;

            Bonds = new Models.Bonds();
            Bonds.Add(new Bond() { ISIN = "Bond1", Price = 100.0, Timestamp = DateTime.Now, YTM = 1.0, Duration = 10.0 });
            Bonds.Add(new Bond() { ISIN = "Bond2", Price = 66.0, Timestamp = DateTime.Now, YTM = 3.0, Duration = 33.0 });
            Bonds.Add(new Bond() { ISIN = "Bond3", Price = 111.0, Timestamp = DateTime.Now, YTM = 6.0, Duration = 2.0 });
            Bonds.Add(new Bond() { ISIN = "Bond4", Price = 123.0, Timestamp = DateTime.Now, YTM = 2.0, Duration = 77.0 });
            Bonds.Add(new Bond() { ISIN = "Bond5", Price = 90.0, Timestamp = DateTime.Now, YTM = 9.0, Duration = 8.0 });

            View.Model = this;
        }
    }
}
