using BondApp.Modules.PriceGrid.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondApp.Modules.PriceGrid.Views
{
    public interface IPriceGridView
    {
        IPriceGridViewModel Model { get; set; }
    }
}
