
using Digimezzo.Foundation.Core.IO;
using Digimezzo.Foundation.Core.Logging;
using Digimezzo.Foundation.Core.Settings;
using Digimezzo.Foundation.Core.Utils;
using Dopamine.Core.Base;
using Dopamine.Services.Dialog;
using Dopamine.Services.Provider;
using Dopamine.Services.Scrobbling;
using Dopamine.Views.FullPlayer.Settings;
using Prism.Commands;

using Prism.Mvvm;
using System;

using System.Collections.ObjectModel;

using System.Threading.Tasks;
using Prism.Ioc;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsOnlineViewModel : BindableBase
    {
        private IContainerProvider container;
        private IProviderService providerService;
        private IDialogService dialogService;
        private ObservableCollection<SearchProvider> searchProviders;
        private SearchProvider selectedSearchProvider;
        private IScrobblingService scrobblingService;
        private bool isLastFmSignInInProgress;
        private bool checkBoxDownloadArtistInformationChecked;
        private bool checkBoxDownloadLyricsChecked;
        private bool checkBoxEnableDiscordRichPresence;
       
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand RemoveCommand { get; set; }
        public DelegateCommand LastfmSignInCommand { get; set; }
        public DelegateCommand LastfmSignOutCommand { get; set; }
        public DelegateCommand CreateLastFmAccountCommand { get; set; }

        public ObservableCollection<SearchProvider> SearchProviders
        {
            get { return this.searchProviders; }
            set
            {
                SetProperty<ObservableCollection<SearchProvider>>(ref this.searchProviders, value);
                this.SelectedSearchProvider = null;
            }
        }

        public SearchProvider SelectedSearchProvider
        {
            get { return this.selectedSearchProvider; }
            set
            {
                SetProperty<SearchProvider>(ref this.selectedSearchProvider, value);
                this.EditCommand.RaiseCanExecuteChanged();
                this.RemoveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CheckBoxDownloadArtistInformationChecked
        {
            get { return this.checkBoxDownloadArtistInformationChecked; }
            set
            {
                SettingsClient.Set<bool>("Lastfm", "DownloadArtistInformation", value);
                SetProperty<bool>(ref this.checkBoxDownloadArtistInformationChecked, value);
            }
        }

        public bool CheckBoxEnableDiscordRichPresence
        {
            get { return this.checkBoxEnableDiscordRichPresence; }
            set
            {
                SettingsClient.Set<bool>("Discord", "EnableDiscordRichPresence", value, true);
                SetProperty<bool>(ref this.checkBoxEnableDiscordRichPresence, value);
            }
        }

        public bool CheckBoxDownloadLyricsChecked
        {
            get { return this.checkBoxDownloadLyricsChecked; }
            set
            {
                SettingsClient.Set<bool>("Lyrics", "DownloadLyrics", value, true);
                SetProperty<bool>(ref this.checkBoxDownloadLyricsChecked, value);
            }
        }

        public bool IsLastFmSignedIn
        {
            get { return this.scrobblingService.SignInState == SignInState.SignedIn; }
        }

        public string LastFmUsername
        {
            get { return this.scrobblingService.Username; }
            set
            {
                this.scrobblingService.Username = value;
            }
        }

        public bool IsLastFmSigningIn
        {
            get { return this.isLastFmSignInInProgress; }
            set
            {
                SetProperty<bool>(ref this.isLastFmSignInInProgress, value);
            }
        }

        public bool IsLastFmSignInError
        {
            get { return this.scrobblingService.SignInState == SignInState.Error; }
        }

        public SettingsOnlineViewModel(IContainerProvider container, IProviderService providerService, IDialogService dialogService, IScrobblingService scrobblingService)
        {
            this.container = container;
            this.providerService = providerService;
            this.dialogService = dialogService;
            this.scrobblingService = scrobblingService;

            this.scrobblingService.SignInStateChanged += (_) =>
            {
                this.IsLastFmSigningIn = false;
                RaisePropertyChanged(nameof(this.IsLastFmSignedIn));
                RaisePropertyChanged(nameof(this.LastFmUsername));
                RaisePropertyChanged(nameof(this.IsLastFmSignInError));
            };

            this.AddCommand = new DelegateCommand(() => this.AddSearchProvider());
            this.EditCommand = new DelegateCommand(() => { this.EditSearchProvider(); }, () => { return this.SelectedSearchProvider != null; });
            this.RemoveCommand = new DelegateCommand(() => { this.RemoveSearchProvider(); }, () => { return this.SelectedSearchProvider != null; });
            this.LastfmSignInCommand = new DelegateCommand(async () =>
            {
                this.IsLastFmSigningIn = true;
                await this.scrobblingService.SignIn();

            });
            this.LastfmSignOutCommand = new DelegateCommand(() => this.scrobblingService.SignOut());
            this.CreateLastFmAccountCommand = new DelegateCommand(() =>
            {
                try
                {
                    Actions.TryOpenLink(Constants.LastFmJoinLink);
                }
                catch (Exception ex)
                {
                    LogClient.Error("Could not open the Last.fm web page. Exception: {0}", ex.Message);
                }
            });

            this.GetSearchProvidersAsync();

            this.providerService.SearchProvidersChanged += (_, __) => this.GetSearchProvidersAsync();

            this.GetCheckBoxesAsync();
        }

        private async void GetSearchProvidersAsync()
        {
            var providersList = await this.providerService.GetSearchProvidersAsync();
            var localProviders = new ObservableCollection<SearchProvider>();

            foreach (SearchProvider provider in providersList)
            {
                localProviders.Add(provider);
            }

            this.SearchProviders = localProviders;
        }

        private void AddSearchProvider()
        {
            SettingsOnlineAddEditSearchProvider view = this.container.Resolve<SettingsOnlineAddEditSearchProvider>();
            view.DataContext = this.container.Resolve<Func<SearchProvider, SettingsOnlineAddEditSearchProviderViewModel>>()(new SearchProvider());

            string dialogTitle = ResourceUtils.GetString("Language_Add");

            this.dialogService.ShowCustomDialog(
                0xe104,
                14,
                dialogTitle,
                view,
                450,
                0,
                false,
                true,
                true,
                true,
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ((SettingsOnlineAddEditSearchProviderViewModel)view.DataContext).AddSearchProviderAsync);
        }

        private void EditSearchProvider()
        {
            SettingsOnlineAddEditSearchProvider view = this.container.Resolve<SettingsOnlineAddEditSearchProvider>();
            view.DataContext = this.container.Resolve<Func<SearchProvider, SettingsOnlineAddEditSearchProviderViewModel>>()(this.selectedSearchProvider);

            string dialogTitle = ResourceUtils.GetString("Language_Edit");

            this.dialogService.ShowCustomDialog(
                0xe104,
                14,
                dialogTitle,
                view,
                450,
                0,
                false,
                true,
                true,
                true,
                ResourceUtils.GetString("Language_Ok"),
                ResourceUtils.GetString("Language_Cancel"),
                ((SettingsOnlineAddEditSearchProviderViewModel)view.DataContext).UpdateSearchProviderAsync);
        }

        private void RemoveSearchProvider()
        {
            if (this.dialogService.ShowConfirmation(0xe11b, 16, ResourceUtils.GetString("Language_Remove"), ResourceUtils.GetString("Language_Confirm_Remove_Online_Search_Provider").Replace("{provider}", this.selectedSearchProvider.Name), ResourceUtils.GetString("Language_Yes"), ResourceUtils.GetString("Language_No")))
            {
                var isRemoveSuccess = this.providerService.RemoveSearchProvider(this.selectedSearchProvider);

                if (!isRemoveSuccess)
                {
                    this.dialogService.ShowNotification(
                        0xe711,
                        16,
                        ResourceUtils.GetString("Language_Error"),
                        ResourceUtils.GetString("Language_Error_Removing_Online_Search_Provider").Replace("{provider}", this.selectedSearchProvider.Name),
                        ResourceUtils.GetString("Language_Ok"),
                        true,
                        ResourceUtils.GetString("Language_Log_File"));
                }
            }
        }

        private async void GetCheckBoxesAsync()
        {
            await Task.Run(() =>
            {
                this.checkBoxDownloadArtistInformationChecked = SettingsClient.Get<bool>("Lastfm", "DownloadArtistInformation");
                this.checkBoxEnableDiscordRichPresence = SettingsClient.Get<bool>("Discord", "EnableDiscordRichPresence");
                this.checkBoxDownloadLyricsChecked = SettingsClient.Get<bool>("Lyrics", "DownloadLyrics");

                string lyricsProviders = SettingsClient.Get<string>("Lyrics", "Providers");
            });
        }
    }
}
