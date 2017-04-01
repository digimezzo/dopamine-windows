using System;
using Prism;
using Prism.Mvvm;
using Prism.Events;
using Dopamine.Common.Prism;

namespace Dopamine.FullPlayerModule.ViewModels
{
    public class NowPlayingScreenLyricsViewModel : BindableBase, IActiveAware
    {
        #region Variables
        private IEventAggregator eventAggregator;
        private bool isActive;
        #endregion

        #region Construction
        public NowPlayingScreenLyricsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }
        #endregion

        #region IActiveAware
        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                SetProperty<bool>(ref this.isActive, value);
                this.eventAggregator.GetEvent<LyricsScreenIsActiveChanged>().Publish(value);
            }
        }

        public event EventHandler IsActiveChanged;
        #endregion
    }
}
