using Dopamine.Core.Logging;
using Dopamine.Common.Presentation.Views.Base;
using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Windows.Input;

namespace Dopamine.Common.Presentation.Views
{
    public partial class VerticalVolumeControls : VolumeControlViewBase
    {
        #region Variables
        private IPlaybackService playBackService;
        #endregion

        #region Properties
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }
        #endregion

        #region Construction
        public VerticalVolumeControls()
        {
            InitializeComponent();

            // We need a parameterless constructor to be able to use this UserControl in other UserControls without dependency injection.
            // So for now there is no better solution than to find the EventAggregator by using the ServiceLocator.
            this.playBackService = ServiceLocator.Current.GetInstance<IPlaybackService>();
        }
        #endregion

        #region Private
        private void StackPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                this.playBackService.Volume = Convert.ToSingle(this.playBackService.Volume + this.CalculateVolumeDelta(e.Delta));
            }
            catch (Exception ex)
            {
                CoreLogger.Current.Error("There was a problem changing the volume by mouse scroll. Exception: {0}", ex.Message);
            }
        }
        #endregion

    }
}
