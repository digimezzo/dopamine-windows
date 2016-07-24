using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Controls;

namespace Dopamine.SettingsModule.Views
{
    public partial class SettingsStartup : UserControl, IView
    {
        #region IView
        public SettingsStartup()
        {
            InitializeComponent();
        }
        #endregion
    }
}
