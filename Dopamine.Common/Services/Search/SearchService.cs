using Dopamine.Core.Base;
using System;
using System.Timers;

namespace Dopamine.Common.Services.Search
{
    public class SearchService : ISearchService
    {
        #region Variables
        private string searchText;
        private Timer searchTimer;
        private double searchTimeoutSeconds = Constants.SearchTimeoutSeconds;
        #endregion

        #region Properties
        public string SearchText
        {
            // Make sure we never return null
            get { return this.searchText != null ? this.searchText : string.Empty; }
            set
            {
                this.searchText = value;
                this.StartSearchTimer();
            }
        }
        #endregion

        #region Events
        public event Action<string> DoSearch = delegate { };
        #endregion

        #region Private
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
        #endregion
    }
}
