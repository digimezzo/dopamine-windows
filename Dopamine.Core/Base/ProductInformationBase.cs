using Dopamine.Core.Packaging;
using System;
using System.Linq;

namespace Dopamine.Core.Base
{
    public abstract class ProductInformationBase
    {
        public static string ApplicationName = "Dopamine";
        public static string Copyright = "Copyright Digimezzo © 2014-" + DateTime.Now.Year;

        public static ExternalComponent[] CommonComponents = {
            new ExternalComponent {
                Name = "Sqlite-net",
                Description = "A minimal library to allow .NET and Mono applications to store data in SQLite 3 databases.",
                Url = "https://github.com/praeclarum/sqlite-net",
                LicenseUrl = "https://github.com/praeclarum/sqlite-net/blob/master/LICENSE.md"
            },
            new ExternalComponent {
                Name = "TagLib#",
                Description = "A library for reading and writing metadata in media files, including video, audio, and photo formats.",
                Url = "https://github.com/mono/taglib-sharp",
                LicenseUrl = "https://github.com/mono/taglib-sharp/blob/master/COPYING"
            },
            new ExternalComponent {
                Name = "Prism",
                Description = "Prism is a framework for building loosely coupled, maintainable, and testable XAML applications in WPF, Windows 10 UWP, and Xamarin Forms.",
                Url = "https://github.com/PrismLibrary/Prism",
                LicenseUrl = "https://github.com/PrismLibrary/Prism/blob/master/LICENSE"
            },
            new ExternalComponent {
                Name = "Unity",
                Description = "A lightweight extensible dependency injection container with support for constructor, property, and method call injection.",
                Url = "https://github.com/unitycontainer/unity",
                LicenseUrl = "https://github.com/unitycontainer/unity/blob/master/LICENSE.txt"
            },
        };

        public abstract ExternalComponent[] SpecificComponents { get; }

        public ExternalComponent[] Components => CommonComponents.Concat(SpecificComponents)
            .ToArray()
            .OrderBy(c => c.Name)
            .ToArray();
    }
}
