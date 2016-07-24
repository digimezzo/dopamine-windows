using Dopamine.Core.Prism;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using System;

namespace Dopamine.OobeModule.ViewModels
{
    public class OobeDonateViewModel : BindableBase, IActiveAware, INavigationAware
    {
        #region Variables
        private bool isActive;
        private IEventAggregator eventAggregator;
        #endregion

        #region Properties
        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty<bool>(ref this.isActive, value); }
        }
        #endregion

        #region Construction
        public OobeDonateViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }
        #endregion

        #region IActiveAware
        public event EventHandler IsActiveChanged;
        #endregion

        #region INavigationAware
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<OobeNavigatedToEvent>().Publish(typeof(OobeDonateViewModel).FullName);
        }
        #endregion
    }
}
