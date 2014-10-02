using BondApp.Modules.PriceGrid.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BondApp.Modules.PriceGrid.Views
{
    /// <summary>
    /// Interaction logic for PriceGridView.xaml
    /// </summary>
    public partial class PriceGridView : UserControl, IPriceGridView
    {
        public PriceGridView()
        {
            InitializeComponent();
        }

        public IPriceGridViewModel Model
        {
            get { return DataContext as IPriceGridViewModel; }
            set { DataContext = value; }
        }
    }
}
