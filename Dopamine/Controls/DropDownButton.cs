using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

/// <summary>
/// This code is based on code from this article: https://www.codeproject.com/Tips/1136681/WPF-Drop-Down-Menu-Button
/// License: https://www.codeproject.com/info/cpol10.aspx
/// </summary>
namespace Dopamine.Controls
{
    public class DropDownButton : ToggleButton
    {
        public DropDownButton()
        {
            // Bind the ToggleButton.IsChecked property to the drop-down's IsOpen property
            Binding binding = new Binding("Menu.IsOpen");
            binding.Source = this;
            this.SetBinding(DropDownButton.IsCheckedProperty, binding);

            this.DataContextChanged += (sender, args) =>
            {
                if (this.Menu != null)
                {
                    this.Menu.DataContext = this.DataContext;
                }
            };
        }

        public ContextMenu Menu
        {
            get { return (ContextMenu)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }
        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu),
            typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null, OnMenuChanged));

        private static void OnMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropDownButton dropDownButton = (DropDownButton)d;
            ContextMenu contextMenu = (ContextMenu)e.NewValue;
            contextMenu.DataContext = dropDownButton.DataContext;
        }

        protected override void OnClick()
        {
            if (this.Menu != null)
            {
                this.Menu.PlacementTarget = this;
                this.Menu.Placement = PlacementMode.Bottom;
                this.Menu.IsOpen = true;
            }
        }
    }
}
