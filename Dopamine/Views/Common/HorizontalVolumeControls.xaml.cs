using Digimezzo.Foundation.Core.Logging;
using Dopamine.Views.Base;
using Dopamine.Services.Playback;
using CommonServiceLocator;
using System;
using System.Windows;
using System.Windows.Input;

namespace Dopamine.Views.Common
{
    public partial class HorizontalVolumeControls : VolumeControlViewBase
    {
        private IPlaybackService playBackService;
      
        public static readonly DependencyProperty ShowPercentProperty = DependencyProperty.Register("ShowPercent", typeof(bool), typeof(HorizontalVolumeControls), new PropertyMetadata(true));
        public static readonly DependencyProperty SliderLengthProperty = DependencyProperty.Register("SliderLength", typeof(double), typeof(HorizontalVolumeControls), new PropertyMetadata(100.0));
    
        public bool ShowPercent
        {
            get { return Convert.ToBoolean(GetValue(ShowPercentProperty)); }

            set { SetValue(ShowPercentProperty, value); }
        }

        public double SliderLength
        {
            get { return Convert.ToDouble(GetValue(SliderLengthProperty)); }

            set { SetValue(SliderLengthProperty, value); }
        }
   
        public HorizontalVolumeControls()
        {
            InitializeComponent();

            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.playBackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
        }

        private void StackPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                this.playBackService.Volume = Convert.ToSingle(this.playBackService.Volume + this.CalculateVolumeDelta(e.Delta));
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem changing the volume by mouse scroll. Exception: {0}", ex.Message);
            }
        }
    }
}
