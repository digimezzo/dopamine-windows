using Dopamine.Core.Base;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace Dopamine.Core.Extensions
{
    public static class IDropInfoExtensions
    {
        public static bool IsDraggingFiles(this IDropInfo dropInfo)
        {
            DataObject dataObject = dropInfo.Data as DataObject;

            return dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop);
        }

        public static bool IsDraggingMediaFiles(this IDropInfo dropInfo)
        {
            DataObject dataObject = dropInfo.Data as DataObject;
            StringCollection filenames = dataObject.GetFileDropList();
            string[] supportedExtensions = FileFormats.SupportedMediaExtensions;

            foreach (string filename in filenames)
            {
                if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDraggingPlaylistFiles(this IDropInfo dropInfo)
        {
            DataObject dataObject = dropInfo.Data as DataObject;
            StringCollection filenames = dataObject.GetFileDropList();
            string[] supportedExtensions = FileFormats.SupportedStaticPlaylistExtensions;

            foreach (string filename in filenames)
            {
                if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDraggingSmartPlaylistFiles(this IDropInfo dropInfo)
        {
            DataObject dataObject = dropInfo.Data as DataObject;
            StringCollection filenames = dataObject.GetFileDropList();
            string[] supportedExtensions = FileFormats.SupportedSmartPlaylistExtensions;

            foreach (string filename in filenames)
            {
                if (supportedExtensions.Contains(System.IO.Path.GetExtension(filename.ToLower())))
                {
                    return true;
                }
            }

            return false;
        }

        public static IList<string> GetDroppedFilenames(this IDropInfo dropInfo)
        {
            DataObject dataObject = dropInfo.Data as DataObject;
            IList<string> filenames = dataObject.GetFileDropList().Cast<string>().ToList();

            return filenames;
        }
    }
}
