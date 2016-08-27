using Dopamine.ControlsModule.ViewModels;
using Dopamine.ControlsModule.Views;
using Prism.Modularity;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.ControlsModule
{
    public class ControlsModule : IModule
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        #endregion

        #region Construction
        public ControlsModule(IRegionManager regionManager, IUnityContainer container)
        {
            this.regionManager = regionManager;
            this.container = container;
        }
        #endregion

        #region IModule
        public void Initialize()
        {
            this.container.RegisterType<object, CollectionPlaybackControls>(typeof(CollectionPlaybackControls).FullName);
            this.container.RegisterType<object, CollectionPlaybackControlsViewModel>(typeof(CollectionPlaybackControlsViewModel).FullName);
            this.container.RegisterType<object, NowPlayingPlaybackControls>(typeof(NowPlayingPlaybackControls).FullName);
            this.container.RegisterType<object, NowPlayingPlaybackControlsViewModel>(typeof(NowPlayingPlaybackControlsViewModel).FullName);
            this.container.RegisterType<object, SearchControl>(typeof(SearchControl).FullName);
            this.container.RegisterType<object, SearchControlViewModel>(typeof(SearchControlViewModel).FullName);
            this.container.RegisterType<object, SpectrumAnalyzerControl>(typeof(SpectrumAnalyzerControl).FullName);
            this.container.RegisterType<object, SpectrumAnalyzerControlViewModel>(typeof(SpectrumAnalyzerControlViewModel).FullName);
            this.container.RegisterType<object, CoverPlayerControls>(typeof(CoverPlayerControls).FullName);
            this.container.RegisterType<object, MicroPlayerControls>(typeof(MicroPlayerControls).FullName);
            this.container.RegisterType<object, NanoPlayerControls>(typeof(NanoPlayerControls).FullName);
            this.container.RegisterType<object, PlayAllControl>(typeof(PlayAllControl).FullName);
            this.container.RegisterType<object, PlayAllControlViewModel>(typeof(PlayAllControlViewModel).FullName);
            this.container.RegisterType<object, ShuffleAllControl>(typeof(ShuffleAllControl).FullName);
            this.container.RegisterType<object, ShuffleAllControlViewModel>(typeof(ShuffleAllControlViewModel).FullName);
            this.container.RegisterType<object, NothingPlayingControl>(typeof(NothingPlayingControl).FullName);
            this.container.RegisterType<object, NothingPlayingControlViewModel>(typeof(NothingPlayingControlViewModel).FullName);
        }
        #endregion
    }
}
