using Digimezzo.Utilities.Packaging;
using Dopamine.Common.Base;
using System.Reflection;

namespace Dopamine.Packager
{
    class Packager
    {
        static void Main(string[] args)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            AssemblyName an = asm.GetName();

            Configuration config;

#if DEBUG
            config = Configuration.Debug;
#else
		   config = Configuration.Release;
#endif

            var worker = new PackageCreator(ProductInformation.ApplicationName, an.Version, config);
            worker.Execute();
        }
    }
}
