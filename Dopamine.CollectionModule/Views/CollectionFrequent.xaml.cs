using Digimezzo.Utilities.Settings;
using Dopamine.Common.Enums;
using Prism.Regions;
using System.Windows.Controls;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionFrequent : UserControl, INavigationAware
    {
        #region Construction
        public CollectionFrequent()
        {
            InitializeComponent();
        }
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
            SettingsClient.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Recent);
        }
        #endregion

    }
}
