using Digimezzo.Utilities.Packaging;
using System;
using System.Reflection;

namespace Dopamine.Common.Base
{
    public sealed class ProductInformation
    {
        #region About
        public static string ApplicationGuid = "75ba9e1e-9eff-4a8e-845e-125dc4318c3b";
        public static string ApplicationDisplayName = "Dopamine";
        public static string ApplicationAssemblyName = "Dopamine";
        public static string Copyright = "Copyright Digimezzo © 2014-" + DateTime.Now.Year;

        public static string FormattedAssemblyVersion
        {
            get
            {
                Assembly asm = Assembly.GetEntryAssembly(); // Returns the assembly of the first executable that was executed
                AssemblyName an = asm.GetName();

                //  {0}: Major Version,
                //  {1}: Minor Version,
                //  {2}: Build Number,
                //  {3}: Revision

                if (an.Version.Revision != 0)
                {
                    return string.Format("{0}.{1}.{2} (Build {3})", an.Version.Major, an.Version.Minor, an.Version.Revision, an.Version.Build);
                }
                else
                {
                    return string.Format("{0}.{1} (Build {2})", an.Version.Major, an.Version.Minor, an.Version.Build);
                }
            }
        }

        public static Version AssemblyVersion
        {
            get
            {
                Assembly asm = Assembly.GetEntryAssembly(); // Returns the assembly of the first executable that was executed
                AssemblyName an = asm.GetName();

                return an.Version;
            }
        }

        public static string VersionTag
        {

            get
            {
#if DEBUG
                return "Preview";
#else
		        return "Release";
#endif
            }
        }
        #endregion

        #region Components
        public static ExternalComponent[] Components = {
            new ExternalComponent {
                Name = "CSCore – .NET Sound Library",
                Description = "A free .NET audio library which is completely written in C#.",
                Url = "https://github.com/filoe/cscore"
            },
            new ExternalComponent {
                Name = "DotNetZip",
                Description = "A FAST, FREE class library and toolset for manipulating zip files. Use VB, C# or any .NET language to easily create, extract, or update zip files.",
                Url = "http://dotnetzip.codeplex.com"
            },
            new ExternalComponent {
                Name = "Font Awesome",
                Description = "Font Awesome by Dave Gandy.",
                Url = "http://fontawesome.io"
            },
            new ExternalComponent {
                Name = "GongSolutions.WPF.DragDrop",
                Description = "An easy to use drag'n'drop framework for WPF.",
                Url = "https://github.com/punker76/gong-wpf-dragdrop"
            },
            new ExternalComponent {
                Name = "NVorbis",
                Description = "A .NET library for decoding Xiph.org Vorbis files.",
                Url = "https://github.com/ioctlLR/NVorbis"
            },
            new ExternalComponent {
                Name = "Prism",
                Description = "Prism provides guidance designed to help you more easily design and build rich, flexible, and easy-to-maintain WPF desktop applications.",
                Url = "http://compositewpf.codeplex.com"
            },
            new ExternalComponent {
                Name = "Sqlite-net",
                Description = "A minimal library to allow .NET and Mono applications to store data in SQLite 3 databases.",
                Url = "https://github.com/praeclarum/sqlite-net"
            },
            new ExternalComponent {
                Name = "TagLib#",
                Description = "A library for reading and writing metadata in media files, including video, audio, and photo formats.",
                Url = "http://www.nuget.org/packages/taglib"
            },
            new ExternalComponent {
                Name = "Unity.WCF",
                Description = "A library that allows the simple integration of Microsoft's Unity IoC container with WCF.",
                Url = "https://github.com/Uriil/unitywcf"
            },
            new ExternalComponent {
                Name = "Unity",
                Description = "A lightweight extensible dependency injection container with support for constructor, property, and method call injection.",
                Url = "https://unity.codeplex.com"
            },
            new ExternalComponent {
                Name = "WiX",
                Description = "Windows Installer XML Toolset.",
                Url = "http://wix.codeplex.com"
            },
            new ExternalComponent {
                Name = "WPF Native Folder Browser",
                Description = "Adds the Windows Vista / Windows 7 Folder Browser Dialog to WPF projects.",
                Url = "http://wpffolderbrowser.codeplex.com"
            },
            new ExternalComponent {
                Name = "WPF Sound Visualization Library",
                Description = "A collection of WPF Controls for graphically displaying data related to sound processing.",
                Url = "http://wpfsvl.codeplex.com"
            }
        };
        #endregion
    }
}
