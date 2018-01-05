using Dopamine.Core.Base;
using Dopamine.Services.Contracts.Search;
using System;
using System.Timers;

namespace Dopamine.Services.Search
{
    public class SearchService : ISearchService
    {
        private string searchText = string.Empty;
        private Timer searchTimer;
        private double searchTimeoutSeconds = Constants.SearchTimeoutSeconds;
      
        public string SearchText
        {
            // Make sure we never return null
            get { return this.searchText != null ? this.searchText : string.Empty; }
            set
            {
                bool isTextChanged = !this.searchText.Trim().Equals(value.Trim());
                this.searchText = value;

                // Only trigger a search if the text has changed
                if (isTextChanged) this.StartSearchTimer();
            }
        }
       
        public event Action<string> DoSearch = delegate { };
       
        private void StartSearchTimer()
        {
            if (this.searchTimer == null)
            {
                this.searchTimer = new Timer();
                this.searchTimer.Interval = TimeSpan.FromSeconds(this.searchTimeoutSeconds).TotalMilliseconds;
                this.searchTimer.Elapsed += new ElapsedEventHandler(this.SearchTimoutHandler);
            }
            else
            {
                this.searchTimer.Stop();
            }

            this.searchTimer.Start();
        }

        private void SearchTimoutHandler(object sender, ElapsedEventArgs e)
        {
            this.searchTimer.Stop();
            if (DoSearch != null)
            {
                DoSearch(this.searchText);
            }
        }
    }
}
