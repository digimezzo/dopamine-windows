namespace Dopamine.Core.Api.Lyrics
{
    public class Lyrics
    {
        #region Variables
        private string text;
        private string source;
        #endregion

        #region Properties
        public string Text
        {
            get { return this.text; }
        }

        public string Source
        {
            get { return this.source; }
        }

        public bool HasText
        {
            get { return !string.IsNullOrWhiteSpace(this.Text); }
        }
        #endregion

        #region Construction
        public Lyrics(string text, string source)
        {
            this.text = !string.IsNullOrWhiteSpace(text) ? text : string.Empty;
            this.source = !string.IsNullOrWhiteSpace(text) ? source : string.Empty;
        }
        #endregion
    }
}
