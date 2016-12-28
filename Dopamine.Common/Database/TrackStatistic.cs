namespace Dopamine.Common.Database
{
    public class TrackStatistic
    {
        #region Properties
        public string Path { get; set; }
        public long DateLastPlayed { get; set; }
        public long PlayCount { get; set; }
        public long SkipCount { get; set; }
        #endregion

        #region Construction
        public TrackStatistic()
        {
            this.Path = string.Empty;
            this.PlayCount = 0;
            this.SkipCount = 0;
        }
        #endregion
    }
}
