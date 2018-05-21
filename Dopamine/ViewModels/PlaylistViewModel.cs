using Prism.Mvvm;

namespace Dopamine.ViewModels
{
    public class PlaylistViewModel : BindableBase
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string SortName
        {
           get { return name.ToLowerInvariant(); }
        }
     
        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Name.Equals(((PlaylistViewModel)obj).Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
