using System.Reflection;

namespace Dopamine.UWP.Base
{
    public sealed  class ProductInformation : Core.Base.ProductInformation
    {
        public static string AssemblyVersion
        {
            get
            {
                Assembly asm = typeof(ProductInformation).GetTypeInfo().Assembly;
                AssemblyName an = asm.GetName();

                //  {0}: Major Version,
                //  {1}: Minor Version,
                //  {2}: Build Number,
                //  {3}: Revision

                return string.Format("{0}.{1}.{2}", an.Version.Major, an.Version.Minor, an.Version.Build);
            }
        }
    }
}
