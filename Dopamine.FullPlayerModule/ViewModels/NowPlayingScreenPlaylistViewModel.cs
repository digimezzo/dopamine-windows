using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Common.Base;
using Dopamine.Common.Prism;
using Dopamine.FullPlayerModule.Views;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenPlaylistViewModel : NowPlayingViewModel
    {
        #region Construction
        public NowPlayingScreenPlaylistViewModel(IUnityContainer container) : base(container)
        {
            this.eventAggregator.GetEvent<RemoveSelectedTracksWithKeyDelete>().Subscribe((screenName) =>
            {
                if (screenName == typeof(NowPlayingScreenPlaylist).FullName) this.RemoveFromNowPlayingCommand.Execute();
            });
        }
        #endregion

        #region Overrides
        protected async override Task LoadedCommandAsync()
        {
            if (this.isFirstLoad)
            {
                this.isFirstLoad = false;

                await Task.Delay(Constants.NowPlayingListLoadDelay);  // Wait for the UI to slide in
                await this.FillListsAsync(); // Fill all the lists
            }
        }
        #endregion
    }
}
