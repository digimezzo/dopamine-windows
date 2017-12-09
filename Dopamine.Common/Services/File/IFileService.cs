using Dopamine.Common.Database;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.File
{
   public delegate void TracksImportedHandler(List<PlayableTrack> tracks, PlayableTrack trackToPlay);

    [ServiceContract(Namespace = "http://Dopamine.FileService")]
    public interface IFileService
    {
        [OperationContract()]
        void ProcessArguments(string[] iArgs);

        Task<List<PlayableTrack>> ProcessFilesAsync(List<string> filenames);
        Task<PlayableTrack> CreateTrackAsync(string path);

        event TracksImportedHandler TracksImported;
        event EventHandler ImportingTracks;
    }
}
