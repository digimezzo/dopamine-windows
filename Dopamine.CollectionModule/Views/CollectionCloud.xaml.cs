using Dopamine.Common.Enums;
using Dopamine.Core.Settings;
using Prism.Mvvm;
using Prism.Regions;
using System.Windows.Controls;

namespace Dopamine.CollectionModule.Views
{
    public partial class CollectionCloud : UserControl, INavigationAware
    {
        #region Construction
        public CollectionCloud()
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
            XmlSettingsClient.Instance.Set<int>("FullPlayer", "SelectedPage", (int)SelectedPage.Recent);
        }
        #endregion

    }
}
