using Dopamine.Services.Search;
using Prism.Mvvm;

namespace Dopamine.ViewModels.Common
{
    public class SearchControlViewModel : BindableBase
    {
        private string searchText;
        private ISearchService searchService;
      
        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                SetProperty<string>(ref this.searchText, value);
                this.searchService.SearchText = value;
            }
        }
      
        public SearchControlViewModel(ISearchService searchService)
        {
            this.searchService = searchService;

            this.searchService.DoSearch += (searchText) => this.UpdateSearchText();

            this.UpdateSearchText();
        }
   
        private void UpdateSearchText()
        {
            this.searchText = this.searchService.SearchText;
            RaisePropertyChanged(nameof(this.SearchText));
        }
    }
}
