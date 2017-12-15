using Digimezzo.Utilities.Log;
using Dopamine.Presentation.Views.Base;
using Dopamine.Services.Contracts.Playback;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Windows.Input;

namespace Dopamine.Presentation.Views
{
    public partial class VerticalVolumeControls : VolumeControlViewBase
    {
        private IPlaybackService playBackService;

        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
   
        public VerticalVolumeControls()
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
