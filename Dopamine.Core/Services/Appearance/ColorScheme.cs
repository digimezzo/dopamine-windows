namespace Dopamine.Core.Services.Appearance
{
    public class ColorScheme
    {
        #region Variables
        private string name;
        private string accentColor;
        private int hashCode = 0;
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

            return this.GetHashCode().Equals(((ColorScheme)obj).GetHashCode());
        }

        public override int GetHashCode()
        {
            if (this.hashCode == 0)
                this.hashCode = (Name + AccentColor).GetHashCode();
            return this.hashCode;
        }
        #endregion
    }
}
