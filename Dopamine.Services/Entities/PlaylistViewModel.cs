using Dopamine.Services.Playlist;
using Prism.Mvvm;
using System;

namespace Dopamine.Services.Entities
{
    public class PlaylistViewModel : BindableBase
    {
        public string Name { get; }

        public string Path { get; }

        public PlaylistType Type { get; }

        public string SortName
        {
           get { return Name.ToLowerInvariant(); }
        }

        public PlaylistViewModel(string name, string path, PlaylistType type)
        {
            this.Name = name;
            this.Path = path;
            this.Type = type;
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

            return this.Name.Equals(((PlaylistViewModel)obj).Name, StringComparison.OrdinalIgnoreCase) & this.Type.Equals(((PlaylistViewModel)obj).Type);
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }
    }
}
