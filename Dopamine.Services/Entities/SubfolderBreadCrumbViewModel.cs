using Prism.Mvvm;
using System.IO;

namespace Dopamine.Services.Entities
{
    public class SubfolderBreadCrumbViewModel : BindableBase
    {
        public string DisplayName { get; }

        public string Path { get; }

        public SubfolderBreadCrumbViewModel(string path)
        {
            this.DisplayName = new DirectoryInfo(path).Name;
            this.Path = path;
        }
    }
}
