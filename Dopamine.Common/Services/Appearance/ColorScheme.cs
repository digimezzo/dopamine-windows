namespace Dopamine.Common.Services.Appearance
{
    public class ColorScheme
    {
        public string Name { get; set; }
        public string AccentColor { get; set; }
   
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
    }
}
