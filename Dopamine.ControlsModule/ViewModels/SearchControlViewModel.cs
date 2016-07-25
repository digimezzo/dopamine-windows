using Dopamine.Common.Services.Search;
using Microsoft.Practices.Prism.Mvvm;

namespace Dopamine.ControlsModule.ViewModels
{
    public class SearchControlViewModel : BindableBase
    {
        #region Variables
        private string searchText;
        private ISearchService searchService;
        #endregion

        #region Properties
        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                SetProperty<string>(ref this.searchText, value);
                this.searchService.SearchText = value;
            }
        }
        #endregion

        #region Construction
        public SearchControlViewModel(ISearchService searchService)
        {
            this.searchService = searchService;

            this.searchService.DoSearch += (searchText) => this.UpdateSearchText();

            this.UpdateSearchText();
        }
        #endregion

        #region Private
        private void UpdateSearchText()
        {
            this.searchText = this.searchService.SearchText;
            OnPropertyChanged(() => this.SearchText);
        }
        #endregion
    }
}
