using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Controls
{
    public class LoveButton : Control
    {
        private Button loveButton;
        private TextBlock heartFill;
        private TextBlock heart;

        public bool Love
        {
            get { return Convert.ToBoolean(GetValue(LoveProperty)); }
            set { SetValue(LoveProperty, value); }
        }

        public static readonly DependencyProperty LoveProperty = 
            DependencyProperty.Register(nameof(Love), typeof(bool), typeof(LoveButton), new PropertyMetadata(false));

        public new double FontSize
        {
            get { return Convert.ToDouble(GetValue(FontSizeProperty)); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static new readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(LoveButton), new PropertyMetadata(14.0));

        static LoveButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LoveButton), new FrameworkPropertyMetadata(typeof(LoveButton)));
        }

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

        private void LoveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Love = !this.Love;
        }

        private void LoveButton_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This prevents other double click actions while rating, like playing the selected song.
            e.Handled = true;
        }
    }
}
