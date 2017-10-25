using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.I18n;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.ViewModels.Common
{
    public class AppearanceLanguageViewModel : BindableBase
    {
        private II18nService i18nService;
        private ObservableCollection<Language> languages;
        private Language selectedLanguage;

        public ObservableCollection<Language> Languages
        {
            get { return this.languages; }
            set { SetProperty<ObservableCollection<Language>>(ref this.languages, value); }
        }

        public Language SelectedLanguage
        {
            get { return this.selectedLanguage; }
            set
            {
                SetProperty<Language>(ref this.selectedLanguage, value);

                if (value != null)
                {
                    SettingsClient.Set<string>("Appearance", "Language", value.Code);
                    Application.Current.Dispatcher.Invoke(() => i18nService.ApplyLanguageAsync(value.Code, true));
                }
            }
        }

        public AppearanceLanguageViewModel(II18nService i18nService)
        {
            this.i18nService = i18nService;

            this.GetLanguagesAsync();

            this.i18nService.LanguagesChanged += (_, __) => this.GetLanguagesAsync();
        }

        private async void GetLanguagesAsync()
        {
            List<Language> languagesList = this.i18nService.GetLanguages();
            ObservableCollection<Language> localLanguages = new ObservableCollection<Language>();

            await Task.Run(() =>
            {
                foreach (Language lang in languagesList)
                {
                    localLanguages.Add(lang);
                }
            });

            this.Languages = localLanguages;
            Language tempLanguage = null;

            await Task.Run(() =>
            {
                string savedLanguageCode = SettingsClient.Get<string>("Appearance", "Language");

                if (!string.IsNullOrEmpty(savedLanguageCode))
                {
                    tempLanguage = this.i18nService.GetLanguage(savedLanguageCode);
                }

                // If SelectedLanguage is null (e.g. when the user deletes a language file), select the default language.
                if (tempLanguage == null)
                {
                    tempLanguage = this.i18nService.GetDefaultLanguage();
                }
            });

            this.selectedLanguage = tempLanguage;
            RaisePropertyChanged(nameof(this.SelectedLanguage));
        }
    }
}
