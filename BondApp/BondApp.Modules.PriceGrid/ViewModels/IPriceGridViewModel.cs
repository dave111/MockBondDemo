using BondApp.Modules.PriceGrid.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondApp.Modules.PriceGrid.ViewModels
{
    public interface IPriceGridViewModel
    {
        IPriceGridView View { get; set; }
    }
}
