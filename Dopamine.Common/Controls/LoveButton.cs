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
        private TextBlock heartFill;
        private TextBlock heart;
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
            this.heartFill = (TextBlock)GetTemplateChild("PART_HeartFill");
            this.heart = (TextBlock)GetTemplateChild("PART_Heart");

            if (this.loveButton != null)
            {
                this.loveButton.Click += LoveButton_Click;
                this.loveButton.PreviewMouseDoubleClick += LoveButton_PreviewMouseDoubleClick;
            }

            if (this.heartFill != null)
            {
                this.heartFill.MouseEnter += HeartFill_MouseEnter;
                this.heartFill.MouseLeave += HeartFill_MouseLeave;
            }

            if (this.heart != null)
            {
                this.heart.MouseEnter += Heart_MouseEnter;
                this.heart.MouseLeave += Heart_MouseLeave;
            }
        }

        private void Heart_MouseEnter(object sender, MouseEventArgs e)
        {
            this.heart.Opacity = 1.0;
        }

        private void Heart_MouseLeave(object sender, MouseEventArgs e)
        {
            this.heart.Opacity = 0.2;
        }

        private void HeartFill_MouseEnter(object sender, MouseEventArgs e)
        {
            this.heartFill.Text = char.ConvertFromUtf32(0xE00C);
        }

        private void HeartFill_MouseLeave(object sender, MouseEventArgs e)
        {
            this.heartFill.Text = char.ConvertFromUtf32(0xE0A5);
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
