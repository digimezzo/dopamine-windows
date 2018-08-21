using System.IO;

namespace Dopamine.Services.Entities
{
    public class SubfolderBreadCrumb
    {
        public string DisplayName { get; }

        public string Path { get; }

        public SubfolderBreadCrumb(string path)
        {
            this.DisplayName = new DirectoryInfo(path).Name;
            this.Path = path;
        }
    }
}
