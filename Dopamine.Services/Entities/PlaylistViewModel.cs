using Prism.Mvvm;

namespace Dopamine.Services.Entities
{
    public class PlaylistViewModel : BindableBase
    {
        public string Name { get; }
        public string Path { get; }

        public string SortName
        {
           get { return Name.ToLowerInvariant(); }
        }

        public PlaylistViewModel(string name, string path)
        {
            this.Name = name;
            this.Path = path;
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
