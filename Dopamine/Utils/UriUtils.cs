using System;

namespace Dopamine.Utils
{
    public class UriUtils
    {
        public static Uri MakePackUri<T>(string relativeFile)
        {
            var a = typeof(T).Assembly;
            var assemblyShortName = a.ToString().Split(',')[0];
            var uriString = $"pack://application:,,,/{assemblyShortName};component/{relativeFile}";
            
            return new Uri(uriString);
        }
    }
}