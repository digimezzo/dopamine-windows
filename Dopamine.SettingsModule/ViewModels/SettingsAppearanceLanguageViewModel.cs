using Dopamine.Common.Services.I18n;
using Dopamine.Core.Settings;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.SettingsModule.ViewModels
{
    public class SettingsAppearanceLanguageViewModel : BindableBase
    {
        #region Variables
        private II18nService i18nService;
        private ObservableCollection<Language> languages;
        private Language selectedLanguage;
        #endregion

        #region Properties
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
                    XmlSettingsClient.Instance.Set<string>("Appearance", "Language", value.Code);
                    Application.Current.Dispatcher.Invoke(() => i18nService.ApplyLanguageAsync(value.Code));
                }

            }
        }
        #endregion

        #region Construction
        public SettingsAppearanceLanguageViewModel(II18nService i18nService)
        {
            this.i18nService = i18nService;

            this.GetLanguagesAsync();

            this.i18nService.LanguagesChanged += (_, __) => this.GetLanguagesAsync();
        }
        #endregion

        #region Private
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
                string savedLanguageCode = XmlSettingsClient.Instance.Get<string>("Appearance", "Language");

                if (!string.IsNullOrEmpty(savedLanguageCode))
                {
                    tempLanguage = this.i18nService.GetLanguage(savedLanguageCode);
                }

                // If, for some reason, SelectedLanguage is Nothing (e.g. when the user 
                // deleted a previously existing language file), select the default language.
                if (tempLanguage == null)
                {
                    tempLanguage = this.i18nService.GetDefaultLanguage();
                }
            });

            // Only set SelectedLanguage when we are sure that it is not Nothing. Otherwise this could trigger strange 
            // behaviour in the setter of the SelectedLanguage Property (because the "value" would be Nothing)
            this.SelectedLanguage = tempLanguage;
        }
        #endregion
    }
}
