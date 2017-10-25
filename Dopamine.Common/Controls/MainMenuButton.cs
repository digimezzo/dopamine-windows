using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class MainMenuButton : RadioButton
    {
        static MainMenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainMenuButton), new FrameworkPropertyMetadata(typeof(MainMenuButton)));
        }
    }
}
