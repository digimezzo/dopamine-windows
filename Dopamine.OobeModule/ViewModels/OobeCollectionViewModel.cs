using Dopamine.Common.Prism;
using Prism;
using Prism.Mvvm;
using Prism.Events;
using Prism.Regions;
using System;

namespace Dopamine.OobeModule.ViewModels
{
    public class OobeCollectionViewModel : BindableBase, IActiveAware, INavigationAware
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
        public OobeCollectionViewModel(IEventAggregator eventAggregator)
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
            this.eventAggregator.GetEvent<OobeNavigatedToEvent>().Publish(typeof(OobeCollectionViewModel).FullName);
        }
        #endregion
    }
}
