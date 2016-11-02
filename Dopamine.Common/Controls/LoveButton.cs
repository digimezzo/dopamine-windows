using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Controls
{
    public class LoveButton : Control
    {
        #region Variables
        private Button loveButton;
        #endregion

        #region Properties
        public bool Love
        {
            get { return Convert.ToBoolean(GetValue(LoveProperty)); }
            set { SetValue(LoveProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty LoveProperty = DependencyProperty.Register("Love", typeof(bool), typeof(LoveButton), new PropertyMetadata(false));
        #endregion

        #region Construction
        static LoveButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoveButton), new FrameworkPropertyMetadata(typeof(LoveButton)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.loveButton = (Button)GetTemplateChild("PART_LoveButton");

            if (this.loveButton != null)
            {
                this.loveButton.Click += LoveButton_Click;
                this.loveButton.PreviewMouseDoubleClick += LoveButton_PreviewMouseDoubleClick;
            }
        }
        #endregion

        #region Event Handlers
        private void LoveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Love = !this.Love;
        }

        private void LoveButton_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This prevents other double click actions while rating, like playing the selected song.
            e.Handled = true;
        }
        #endregion
    }
}
