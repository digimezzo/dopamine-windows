using Digimezzo.Utilities.Log;
using Dopamine.Common.Base;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class ScalingTextBlock : TextBlock
    {
        #region Properties
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
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty MinFontSizeProperty = DependencyProperty.Register("MinFontSize", typeof(double), typeof(ScalingTextBlock), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty MaxFontSizeProperty = DependencyProperty.Register("MaxFontSize", typeof(double), typeof(ScalingTextBlock), new PropertyMetadata(Constants.GlobalFontSize * 2));
        #endregion

        #region Construction
        static ScalingTextBlock()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalingTextBlock), new FrameworkPropertyMetadata(typeof(ScalingTextBlock)));
        }
        #endregion

        #region Overrides
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
        #endregion

        #region Private
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
        #endregion
    }
}