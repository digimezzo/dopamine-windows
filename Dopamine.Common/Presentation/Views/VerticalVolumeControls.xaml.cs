using Dopamine.Common.Services.Playback;
using Dopamine.Core.Logging;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Presentation.Views
{
    public partial class VerticalVolumeControls : UserControl, IView
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

        #region "Private"
        private void StackPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                this.playBackService.Volume = Convert.ToSingle(this.playBackService.Volume + ((double)e.Delta / 5000));
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem changing the volume by mouse scroll. Exception: {0}", ex.Message);
            }
        }
        #endregion

    }
}
