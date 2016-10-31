using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class LoveButton : Control
    {
        #region Construction
        static LoveButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoveButton), new FrameworkPropertyMetadata(typeof(LoveButton)));
        }
        #endregion
    }
}
