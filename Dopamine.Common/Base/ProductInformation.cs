using Dopamine.Core.Packaging;
using System.Linq;

namespace Dopamine.Common.Base
{
    public sealed class ProductInformation : Core.Base.ProductInformation
    {
        #region About
        public static string ApplicationGuid = "75ba9e1e-9eff-4a8e-845e-125dc4318c3b";
        public static string ApplicationAssemblyName = "Dopamine";
        #endregion

        #region Components
        public static ExternalComponent[] SpecificComponents = {
            new ExternalComponent {
                Name = "CSCore – .NET Sound Library",
                Description = "A free .NET audio library which is completely written in C#.",
                Url = "https://github.com/filoe/cscore",
                LicenseUrl = "https://github.com/filoe/cscore/blob/master/license.md"
            },
            new ExternalComponent {
                Name = "DotNetZip",
                Description = "A FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.",
                Url = "http://dotnetzip.codeplex.com",
                LicenseUrl = "http://dotnetzip.codeplex.com/license"
            },
            new ExternalComponent {
                Name = "Font Awesome",
                Description = "Font Awesome by Dave Gandy.",
                Url = "http://fontawesome.io",
                LicenseUrl = "http://fontawesome.io/license/"
            },
            new ExternalComponent {
                Name = "GongSolutions.WPF.DragDrop",
                Description = "An easy to use drag'n'drop framework for WPF.",
                Url = "https://github.com/punker76/gong-wpf-dragdrop",
                LicenseUrl = "https://github.com/punker76/gong-wpf-dragdrop/blob/dev/LICENSE"
            },
            new ExternalComponent {
                Name = "NVorbis",
                Description = "A .NET library for decoding Xiph.org Vorbis files.",
                Url = "https://github.com/ioctlLR/NVorbis",
                LicenseUrl = "https://github.com/ioctlLR/NVorbis/blob/master/LICENSE"
            },
            new ExternalComponent {
                Name = "Prism",
                Description = "Prism is a framework for building loosely coupled, maintainable, and testable XAML applications in WPF, Windows 10 UWP, and Xamarin Forms.",
                Url = "https://github.com/PrismLibrary/Prism",
                LicenseUrl = "https://github.com/PrismLibrary/Prism/blob/master/LICENSE"
            },
            new ExternalComponent {
                Name = "Unity.WCF",
                Description = "A library that allows the simple integration of Microsoft's Unity IoC container with WCF.",
                Url = "https://github.com/Uriil/unitywcf",
                LicenseUrl = "https://github.com/Uriil/unitywcf/blob/master/LICENSE"
            },
            new ExternalComponent {
                Name = "Unity",
                Description = "A lightweight extensible dependency injection container with support for constructor, property, and method call injection.",
                Url = "https://github.com/unitycontainer/unity",
                LicenseUrl = "https://github.com/unitycontainer/unity/blob/master/LICENSE.txt"
            },
            new ExternalComponent {
                Name = "WiX",
                Description = "Windows Installer XML Toolset.",
                Url = "http://wix.codeplex.com",
                LicenseUrl = "http://wix.codeplex.com/license"
            },
            new ExternalComponent {
                Name = "WPF Native Folder Browser",
                Description = "Adds the Windows Vista / Windows 7 Folder Browser Dialog to WPF projects.",
                Url = "http://wpffolderbrowser.codeplex.com",
                LicenseUrl = "http://wpffolderbrowser.codeplex.com/license"
            },
            new ExternalComponent {
                Name = "WPF Sound Visualization Library",
                Description = "A collection of WPF Controls for graphically displaying data related to sound processing.",
                Url = "http://wpfsvl.codeplex.com",
                LicenseUrl = "http://wpfsvl.codeplex.com/license"
            }
        };

        public static ExternalComponent[] Components
        {
            get
            {
                return CommonComponents.Concat(SpecificComponents).ToArray().OrderBy(c => c.Name).ToArray();
            }
        }
        #endregion
    }
}
