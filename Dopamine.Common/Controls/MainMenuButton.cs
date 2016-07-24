using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class MainMenuButton : RadioButton
    {
        #region Construction
        static MainMenuButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainMenuButton), new FrameworkPropertyMetadata(typeof(MainMenuButton)));
        }
        #endregion
    }
}
