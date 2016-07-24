using System.Windows;
using System.Windows.Media;

namespace Dopamine.Core.Utils
{
    public static class ResourceUtils
    {
        public static string GetStringResource(string resourceName)
        {
            return Application.Current.TryFindResource(resourceName).ToString();
        }

        public static Geometry GetGeometryResource(string resourceName)
        {
            return (Geometry)Application.Current.TryFindResource(resourceName);
        }
    }
}
