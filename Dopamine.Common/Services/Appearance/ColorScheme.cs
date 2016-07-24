namespace Dopamine.Common.Services.Appearance
{
    public class ColorScheme
    {
        #region Variables
        private string name;
        private string accentColor;
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string AccentColor
        {
            get { return this.accentColor; }
            set { this.accentColor = value; }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Name.Equals(((ColorScheme)obj).Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
        #endregion
    }

}
