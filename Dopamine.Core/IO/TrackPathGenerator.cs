using Digimezzo.Foundation.Core.Utils;

namespace Dopamine.Core.IO
{
    public static class TrackPathGenerator
    {
        public static string GenerateFullTrackPath(string playlistPath, string trackPath)
                {
                    var fullPath = string.Empty;
                    string playlistDirectory = System.IO.Path.GetDirectoryName(playlistPath);
        
                    if (FileUtils.IsAbsolutePath(trackPath))
                    {
                        // The line contains the full path.
                        fullPath = trackPath;
                    }
                    else
                    {
                        // The line contains a relative path, let's construct the full path by ourselves.
                        string tempFullPath = string.Empty;
        
                        if (trackPath.StartsWith(@"\\"))
                        {
                            // This is a network path. Just return as is.
                            return trackPath;
                        }
        
                        if (trackPath.StartsWith(@"\"))
                        {
                            // Path starts with "\": add preceeding "." to make it a valid relative path.
                            trackPath = "." + trackPath;
                        }
        
                        tempFullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(playlistDirectory, trackPath));
        
                        if (!string.IsNullOrEmpty(tempFullPath) && FileUtils.IsAbsolutePath(tempFullPath))
                        {
                            fullPath = tempFullPath;
                        }
                    }
        
                    return fullPath;
                }
    }
}
