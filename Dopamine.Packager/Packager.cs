using Digimezzo.Utilities.Packaging;
using Dopamine.Core.Base;
using System;
using System.Reflection;

namespace Dopamine.Packager
{
    class Packager
    {
        static void Main(string[] args)
        {
            Assembly asm = Assembly.GetEntryAssembly();
            AssemblyName an = asm.GetName();

            bool proceed = true;

#if DEBUG
            proceed = false;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The project was built in DEBUG. Do you want to proceed? [Y/N]");
            Console.ForegroundColor = ConsoleColor.Gray;
            ConsoleKeyInfo info = Console.ReadKey();
            Console.Write(Environment.NewLine);
            Console.Write(Environment.NewLine);

            if (info.Key == ConsoleKey.Y)
            {
                proceed = true;
               
            }   
#endif

            if (proceed)
            {
                var worker = new PackageCreator(ProductInformation.ApplicationName, an.Version);
                worker.ExecuteAsync().Wait();
            }
        }
    }
}
