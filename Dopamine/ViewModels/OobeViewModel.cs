using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Log;
using Dopamine.Common.Enums;
using Dopamine.Common.Services.Dialog;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace Dopamine.ViewModels
{
    public class OobeViewModel : BindableBase
    {
        private bool isOverlayVisible;
        private bool showButtonGoBack;
        private bool showButtonFinish;

        private OobePage selectedOobePage;

        public DelegateCommand GoBackCommand { get; set; }
        public DelegateCommand GoForwardCommand { get; set; }
        public DelegateCommand<string> OpenLinkCommand { get; set; }

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

        public OobeViewModel(IDialogService dialogService)
        {
            dialogService.DialogVisibleChanged += (isDialogVisible) => { this.IsOverlayVisible = isDialogVisible; };

            this.GoBackCommand = new DelegateCommand(() => this.GoBack());
            this.GoForwardCommand = new DelegateCommand(() => this.GoForward());

            this.OpenLinkCommand = new DelegateCommand<string>((url) =>
            {
                try
                {
                    Actions.TryOpenLink(url);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open link {0}. Exception: {1}", url, ex.Message);
                }
            });
        }

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
    }
}
