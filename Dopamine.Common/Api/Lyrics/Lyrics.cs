namespace Dopamine.Common.Api.Lyrics
{
    public enum SourceTypeEnum
    {
        Audio,
        Lrc,
        Online
    }

    public class Lyrics
    {
        private string text;
        private string source;

        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        public string Source
        {
            get { return this.source; }
        }

        public bool HasText
        {
            get { return !string.IsNullOrWhiteSpace(this.Text); }
        }

        public bool HasSource
        {
            get { return !string.IsNullOrWhiteSpace(this.source); }
        }

        public SourceTypeEnum SourceType { get; set; }

        public Lyrics(string text, string source)
        {
            this.text = text;
            this.source = source;
        }
    }
}
