using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class SubMenuButton : RadioButton
    {
        static SubMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SubMenuButton), new FrameworkPropertyMetadata(typeof(SubMenuButton)));
        }
    }
}
