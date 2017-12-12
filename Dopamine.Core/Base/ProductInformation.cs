using Digimezzo.Utilities.Packaging;
using System;

namespace Dopamine.Core.Base
{
    public sealed class ProductInformation
    {
        public static string ApplicationGuid = "75ba9e1e-9eff-4a8e-845e-125dc4318c3b";
        public static string ApplicationName = "Dopamine";
        public static string Copyright = "Copyright Digimezzo © 2014-" + DateTime.Now.Year;

        public static readonly ExternalComponent[] Components =
        {
            new ExternalComponent
            {
                Name = "CSCore – .NET Sound Library",
                Description = "A free .NET audio library which is completely written in C#.",
                Url = "https://github.com/filoe/cscore",
                LicenseUrl = "https://github.com/filoe/cscore/blob/master/license.md"
            },
            new ExternalComponent
            {
                Name = "CSCore.Ffmpeg",
                Description = "A free .NET audio library which is completely written in C#.",
                Url = "https://github.com/filoe/cscore/tree/master/CSCore.Ffmpeg",
                LicenseUrl = "https://www.gnu.org/licenses/old-licenses/lgpl-2.1.html"
            },
            new ExternalComponent
            {
                Name = "DotNetZip",
                Description =
                    "A FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.",
                Url = "http://dotnetzip.codeplex.com",
                LicenseUrl = "http://dotnetzip.codeplex.com/license"
            },
            new ExternalComponent
            {
                Name = "FFmpeg",
                Description = "A collection of libraries and tools to process multimedia content such as audio, video, subtitles and related metadata.",
                Url = "https://github.com/FFmpeg/FFmpeg",
                LicenseUrl = "https://github.com/FFmpeg/FFmpeg/blob/master/LICENSE.md"
            },
            new ExternalComponent
            {
                Name = "Font Awesome",
                Description = "Font Awesome by Dave Gandy.",
                Url = "http://fontawesome.io",
                LicenseUrl = "http://fontawesome.io/license/"
            },
            new ExternalComponent
            {
                Name = "GongSolutions.WPF.DragDrop",
                Description = "An easy to use drag'n'drop framework for WPF.",
                Url = "https://github.com/punker76/gong-wpf-dragdrop",
                LicenseUrl = "https://github.com/punker76/gong-wpf-dragdrop/blob/dev/LICENSE"
            },
            new ExternalComponent
            {
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
                Name = "Unity",
                Description = "A lightweight extensible dependency injection container with support for constructor, property, and method call injection.",
                Url = "https://github.com/unitycontainer/unity",
                LicenseUrl = "https://github.com/unitycontainer/unity/blob/master/LICENSE.txt"
            },
            new ExternalComponent
            {
                Name = "Unity.WCF",
                Description =
                    "A library that allows the simple integration of Microsoft's Unity IoC container with WCF.",
                Url = "https://github.com/Uriil/unitywcf",
                LicenseUrl = "https://github.com/Uriil/unitywcf/blob/master/LICENSE"
            },
            new ExternalComponent
            {
                Name = "WiX",
                Description = "Windows Installer XML Toolset.",
                Url = "http://wix.codeplex.com",
                LicenseUrl = "http://wix.codeplex.com/license"
            },
            new ExternalComponent
            {
                Name = "WPF Native Folder Browser",
                Description = "Adds the Windows Vista / Windows 7 Folder Browser Dialog to WPF projects.",
                Url = "http://wpffolderbrowser.codeplex.com",
                LicenseUrl = "http://wpffolderbrowser.codeplex.com/license"
            },
            new ExternalComponent
            {
                Name = "WPF Sound Visualization Library",
                Description =
                    "A collection of WPF Controls for graphically displaying data related to sound processing.",
                Url = "http://wpfsvl.codeplex.com",
                LicenseUrl = "http://wpfsvl.codeplex.com/license"
            }
        };
    }
}
