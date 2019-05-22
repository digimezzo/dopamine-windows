using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Base;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Controls
{
    public class ScalingTextBlock : TextBlock
    {
        public double MinFontSize
        {
            get { return Convert.ToDouble(GetValue(MinFontSizeProperty)); }
            set { SetValue(MinFontSizeProperty, value); }
        }

        public double MaxFontSize
        {
            get { return Convert.ToDouble(GetValue(MaxFontSizeProperty)); }
            set { SetValue(MaxFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MinFontSizeProperty = DependencyProperty.Register("MinFontSize", typeof(double), typeof(ScalingTextBlock), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty MaxFontSizeProperty = DependencyProperty.Register("MaxFontSize", typeof(double), typeof(ScalingTextBlock), new PropertyMetadata(Constants.GlobalFontSize * 2));

        static ScalingTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalingTextBlock), new FrameworkPropertyMetadata(typeof(ScalingTextBlock)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.SetFontSize();

            // TextBlock doesn't have a TexChanged event. This adds possibility to detect that the Text Property has changed.
            // Because there is a limited amount of these TextBlocks in the application, this DependencyPropertyDescriptor should
            // not cause any memory leaks.
            DependencyPropertyDescriptor dp = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));

            dp.AddValueChanged(this, (object a, EventArgs b) =>
            {
                this.SetFontSize();
            });

            this.Loaded += ScalingTextBlock_Loaded;
        }

        private void ScalingTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetFontSize();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.SetFontSize();
        }

        private void SetFontSize()
        {
            this.Visibility = Visibility.Hidden;

            try
            {
                double increment = (this.MaxFontSize - this.MinFontSize) / 3;

                this.FontSize = this.MaxFontSize;

                while (this.FontSize > this.MinFontSize & this.TextIsTooLarge())
                {
                    this.FontSize -= increment;
                }

                if (this.FontSize < this.MinFontSize) this.FontSize = this.MinFontSize;
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not set the font size. Setting minimum font size. Exception: {0}", ex.Message);
                this.FontSize = this.MinFontSize;
            }

            this.Visibility = Visibility.Visible;
        }

        private bool TextIsTooLarge()
        {
            try
            {
                this.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not find out if the text is too large. Exception: {0}", ex.Message);
                return true;
            }

            return this.ActualWidth < this.DesiredSize.Width;
        }
    }
}