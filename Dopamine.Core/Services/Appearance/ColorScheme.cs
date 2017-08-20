namespace Dopamine.Core.Services.Appearance
{
    public class ColorScheme
    {
        #region Properties
        public string Name { get; set; }
        public string AccentColor { get; set; }
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
