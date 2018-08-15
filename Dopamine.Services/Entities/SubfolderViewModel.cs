using Dopamine.Core.Extensions;

namespace Dopamine.Services.Entities
{
    public class SubfolderViewModel
    {
        public string DisplayName { get; }

        public string Path { get; }

        public string SafePath { get; }

        public bool IsOneUpFolder { get; }

        public SubfolderViewModel(string path, bool isOneUpSubfolder)
        {
            this.Path = path;
            this.SafePath = path.ToSafePath();
            this.DisplayName = isOneUpSubfolder ? ".." : System.IO.Path.GetFileName(path);
            this.IsOneUpFolder = isOneUpSubfolder;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return string.Equals(this.SafePath, ((SubfolderViewModel)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return this.SafePath.GetHashCode();
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
