using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dopamine.Core.IO
{
    public sealed class FileOperations
    {
        public static void TryDirectoryRecursiveGetFiles(string sourcePath, List<string> files, string[] validExtensions, ConcurrentQueue<Exception> exceptions)
        {
            try
            {
                // Process the list of files found in the directory.
                string[] fileEntries = null;

                try
                {
                    fileEntries = Directory.GetFiles(sourcePath);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }

                if (fileEntries != null && fileEntries.Count() > 0)
                {
                    foreach (string fileName in fileEntries)
                    {
                        try
                        {
                            // Only add the file if it has an extension contained in iValidExtensions
                            if (validExtensions.Contains(Path.GetExtension(fileName.ToLower())))
                            {
                                files.Add(fileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions.Enqueue(ex);
                        }
                    }
                }

                // Recurse into subdirectories of this directory. 
                string[] subdirectoryEntries = null;

                try
                {
                    subdirectoryEntries = Directory.GetDirectories(sourcePath);
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }

                if (subdirectoryEntries != null && subdirectoryEntries.Count() > 0)
                {

                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        try
                        {
                            TryDirectoryRecursiveGetFiles(subdirectory, files, validExtensions, exceptions);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Enqueue(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Enqueue(ex);
            }
        }

        public static long GetFileSize(string path)
        {
            return new System.IO.FileInfo(path).Length;
        }

        public static long GetDateModified(string path)
        {
            return new System.IO.FileInfo(path).LastWriteTime.Ticks;
        }

        public static long GetDateCreated(string path)
        {
            return new System.IO.FileInfo(path).CreationTime.Ticks;
        }

        public static bool IsPathTooLong(string path)
        {
            bool retVal = false;

            // The fully qualified file name must be less than 260 characters, and the 
            // directory name must be less than 248 characters."
            string[] pieces = path.Split('\\');

            if (path.Length >= 248)
            {
                retVal = true;
            }

            return retVal;
        }

        public static bool IsAbsolutePath(string iString)
        {

            Regex regex = new Regex("^(([a-zA-Z]:\\\\)|(//)).*");
            Match match = regex.Match(iString);

            return match.Success;
        }

        public static string SanitizeFilename(string iInput)
        {

            string retVal = string.Empty;
            string replaceStr = string.Empty;

            // Invalid characters for filenames: \ / : * ? " < > |

            retVal = iInput.Replace("\\", replaceStr);
            retVal = retVal.Replace("/", replaceStr);
            retVal = retVal.Replace(":", replaceStr);
            retVal = retVal.Replace("*", replaceStr);
            retVal = retVal.Replace("?", replaceStr);
            retVal = retVal.Replace("\"", replaceStr);
            retVal = retVal.Replace("<", replaceStr);
            retVal = retVal.Replace(">", replaceStr);
            retVal = retVal.Replace("|", replaceStr);

            return retVal;
        }
    }
}
