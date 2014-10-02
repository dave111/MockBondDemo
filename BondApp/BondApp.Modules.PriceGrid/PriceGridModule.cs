using BondApp.Modules.PriceGrid.ViewModels;
using BondApp.Modules.PriceGrid.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BondApp.Modules.PriceGrid
{
    public class PriceGridModule : IModule
    {
        private IRegionManager regionManager;
        private IUnityContainer container;

        public void Initialize()
        {
            RegisterViewsAndServices();
            if (regionManager.Regions.ContainsRegionWithName("GridRegion"))
                regionManager.Regions["GridRegion"].Add(container.Resolve<IPriceGridViewModel>().View);
        }

        public PriceGridModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.container = container;
            this.regionManager = regionManager;
        }

        protected void RegisterViewsAndServices()
        {
            container.RegisterType<IPriceGridView, PriceGridView>();
            container.RegisterType<IPriceGridViewModel, PriceGridViewModel>();
        }
    }
}
