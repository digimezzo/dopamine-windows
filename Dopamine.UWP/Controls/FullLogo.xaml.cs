using Dopamine.UWP.Base;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Controls
{
    public sealed partial class FullLogo : UserControl
    {
        #region Properties
        public string ApplicationDisplayName
        {
            get { return ProductInformation.ApplicationDisplayName.ToLower(); }
        }
        #endregion

        #region Construction
        public FullLogo()
        {
            this.InitializeComponent();
        }
        #endregion
    }
}
