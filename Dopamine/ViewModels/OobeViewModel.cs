using Dopamine.Common.Services.Dialog;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels
{
    public class OobeViewModel : BindableBase
    {
        #region Variables
        private bool isOverlayVisible;
        private bool showButtonGoBack;
        private bool showButtonFinish;

        private enum OobePage
        {
            Welcome = 0,
            Language = 1,
            Appearance = 2,
            Collection = 3,
            Donate = 4,
            Finish = 5
        }

        private OobePage selectedOobePage;
        #endregion

        #region Commands
        public DelegateCommand GoBackCommand { get; set; }
        public DelegateCommand GoForwardCommand { get; set; }
        #endregion

        #region Properties
        public bool IsOverlayVisible
        {
            get { return this.isOverlayVisible; }
            set { SetProperty<bool>(ref this.isOverlayVisible, value); }
        }

        public bool ShowButtonGoBack
        {
            get { return this.showButtonGoBack; }
            set { SetProperty<bool>(ref this.showButtonGoBack, value); }
        }

        public bool ShowButtonFinish
        {
            get { return this.showButtonFinish; }
            set { SetProperty<bool>(ref this.showButtonFinish, value); }
        }

        public Int32 SelectedOobePageIndex
        {
            get { return (Int32)this.selectedOobePage; }
        }

        public bool CanGoBack
        {
            get
            {
                if (this.selectedOobePage.Equals(OobePage.Welcome) || this.selectedOobePage.Equals(OobePage.Language))
                {
                    return false;
                }

                return true;
            }
        }

        public bool CanFinish
        {
            get
            {
                if (this.selectedOobePage.Equals(OobePage.Finish))
                {
                    return true;
                }

                return false;
            }
        }
        #endregion

        #region Construction
        public OobeViewModel(IDialogService dialogService)
        {
            dialogService.DialogVisibleChanged += (isDialogVisible) => { this.IsOverlayVisible = isDialogVisible; };

            this.GoBackCommand = new DelegateCommand(() => this.GoBack());
            this.GoForwardCommand = new DelegateCommand(() => this.GoForward());
        }
        #endregion

        #region Functions
        private void SetSelectedOobePage(OobePage page)
        {
            this.selectedOobePage = page;
            RaisePropertyChanged(nameof(this.SelectedOobePageIndex));
        }

        private void GoBack()
        {
            switch (this.selectedOobePage)
            {
                case OobePage.Welcome:
                    // Do nothing
                    break;
                case OobePage.Language:
                    this.SetSelectedOobePage(OobePage.Welcome);
                    break;
                case OobePage.Appearance:
                    this.SetSelectedOobePage(OobePage.Language);
                    break;
                case OobePage.Collection:
                    this.SetSelectedOobePage(OobePage.Appearance);
                    break;
                case OobePage.Donate:
                    this.SetSelectedOobePage(OobePage.Collection);
                    break;
                case OobePage.Finish:
                    this.SetSelectedOobePage(OobePage.Donate);
                    break;
                default:
                    break;
            }

            RaisePropertyChanged(nameof(this.CanGoBack));
            RaisePropertyChanged(nameof(this.CanFinish));
        }

        private void GoForward()
        {
            switch (this.selectedOobePage)
            {
                case OobePage.Welcome:
                    this.SetSelectedOobePage(OobePage.Language);
                    break;
                case OobePage.Language:
                    this.SetSelectedOobePage(OobePage.Appearance);
                    break;
                case OobePage.Appearance:
                    this.SetSelectedOobePage(OobePage.Collection);
                    break;
                case OobePage.Collection:
                    this.SetSelectedOobePage(OobePage.Donate);
                    break;
                case OobePage.Donate:
                    this.SetSelectedOobePage(OobePage.Finish);
                    break;
                case OobePage.Finish:
                    // Do nothing
                    break;
                default:
                    break;
            }

            RaisePropertyChanged(nameof(this.CanGoBack));
            RaisePropertyChanged(nameof(this.CanFinish));
        }
        #endregion
    }
}
