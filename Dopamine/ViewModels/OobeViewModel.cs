using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Services.Dialog;
using Dopamine.Core.Prism;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Dopamine.ViewModels
{
    public class OobeViewModel : BindableBase
    {
        #region Variables
        private IDialogService dialogService;
        private IEventAggregator eventAggregator;
        private bool isOverlayVisible;
        private SlideDirection slideDirection;
        #endregion

        #region Properties
        public bool IsOverlayVisible
        {
            get { return this.isOverlayVisible; }
            set { SetProperty<bool>(ref this.isOverlayVisible, value); }
        }

        public SlideDirection SlideDirection
        {
            get { return this.slideDirection; }
            set { SetProperty<SlideDirection>(ref this.slideDirection, value); }
        }
        #endregion

        #region Construction
        public OobeViewModel(IDialogService dialogService, IEventAggregator eventAggregator)
        {
            this.dialogService = dialogService;
            this.eventAggregator = eventAggregator;

            this.dialogService.DialogVisibleChanged += (isDialogVisible) => { this.IsOverlayVisible = isDialogVisible; };

            this.eventAggregator.GetEvent<ChangeOobeSlideDirectionEvent>().Subscribe((slideDirection) => { this.SlideDirection = slideDirection; });

            // Initial slide direction
            this.SlideDirection = SlideDirection.RightToLeft;
        }
        #endregion
    }
}
