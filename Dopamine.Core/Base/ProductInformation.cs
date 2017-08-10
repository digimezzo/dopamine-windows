using Dopamine.Core.Packaging;
using System;

namespace Dopamine.Core.Base
{
    public abstract class ProductInformation
    {
        public static string ApplicationDisplayName = "Dopamine";
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
            }
        };
    }
}
