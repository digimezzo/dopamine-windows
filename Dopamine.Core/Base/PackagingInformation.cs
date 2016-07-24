using Dopamine.Core.Base;

namespace Dopamine.Core.Base
{
    public class PackagingInformation
    {
        public static string GetPackageFileName(VersionInfo versionInfo)
        {
            string packageName = string.Format("{0} {1}", ProductInformation.ApplicationAssemblyName, versionInfo.Version.ToString());

            if (versionInfo.Configuration == Configuration.Debug)
            {
                packageName += " Preview";
            }

            return packageName;
        }

        public static string GetInstallablePackageFileExtesion()
        {
            return ".msi";
        }

        public static string GetPortablePackageFileExtesion()
        {
            return ".zip";
        }

        public static string GetUpdatePackageFileExtension()
        {
            return ".update";
        }

    }
}
