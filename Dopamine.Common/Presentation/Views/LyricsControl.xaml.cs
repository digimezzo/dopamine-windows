using Dopamine.Common.Presentation.Utils;
using Dopamine.Core.Logging;
using Dopamine.Core.Prism;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class LyricsControl : UserControl
    {
        #region Variables
        private IEventAggregator eventAggregator;
        private ListBox lyricsBox;
        #endregion

        #region Contruction
        public LyricsControl()
        {
            InitializeComponent();

            this.eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            this.eventAggregator.GetEvent<ScrollToHighlightedLyricsLine>().Subscribe((_) => this.ScrollToHighlightedLyricsLineAsync());
        }
        #endregion

        #region Private
        private void LyricsBox_Loaded(object sender, RoutedEventArgs e)
        {
            // This is a workaround to be able to access the LyricsBox_Loaded which is in the DataTemplate.
            try
            {
                this.lyricsBox = sender as ListBox;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not get lyricsBox ListBox from the DataTemplate. Exception: {0}", ex.Message);
            }
        }

        private async void ScrollToHighlightedLyricsLineAsync()
        {
            if (this.lyricsBox == null) return;

            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await ScrollUtils.ScrollToHighlightedLyricsLineAsync(this.lyricsBox);
                });
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not scroll to the highlighted lyrics line. Exception: {1}", ex.Message);
            }
        }
        #endregion  
    }
}