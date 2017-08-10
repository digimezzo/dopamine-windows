namespace Dopamine.Core.Packaging
{
    public class ExternalComponent
    {
        #region Variables
        private string name;
        private string description;
        private string url;
        private string licenseUrl;
        #endregion

        #region Properties
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public string LicenseUrl
        {
            get { return licenseUrl; }
            set { licenseUrl = value; }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.name;
        }
        #endregion
    }
}