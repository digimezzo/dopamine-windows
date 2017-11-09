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
            var worker = new PackageCreator(ProductInformation.ApplicationName, an.Version);
            worker.ExecuteAsync().Wait();
        }
    }
}
