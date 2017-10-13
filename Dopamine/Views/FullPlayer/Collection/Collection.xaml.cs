using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.FullPlayer.Collection
{
    public partial class Collection : UserControl
    {
        public Collection()
        {
            InitializeComponent();
        }

        private void AlignSpectrumAnalyzer()
        {
            // This makes sure the spectrum analyzer is centered on the screen, based on the left pixel.
            // When we align center, alignment is sometimes (depending on the width of the screen) done
            // on a half pixel. This causes a blurry spectrum analyzer.
            try
            {
                this.SpectrumAnalyzer.Margin = new Thickness(Convert.ToInt32(this.ActualWidth / 2) - Convert.ToInt32(this.SpectrumAnalyzer.ActualWidth / 2), 0, 0, 0);
            }
            catch (Exception)
            {
                // Swallow this exception
            }
        }

        private void SpectrumAnalyzer_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.AlignSpectrumAnalyzer();
        }
    }
}
