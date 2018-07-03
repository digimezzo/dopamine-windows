using Dopamine.Services.Entities;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Services.File
{
    public delegate void TracksImportedHandler(List<TrackViewModel> tracks, TrackViewModel trackToPlay);

    [ServiceContract(Namespace = "http://Dopamine.FileService")]
    public interface IFileService
    {
        [OperationContract()]
        void ProcessArguments(string[] iArgs);

        Task<List<TrackViewModel>> ProcessFilesAsync(List<string> filenames);

        Task<TrackViewModel> CreateTrackAsync(string path);

        event TracksImportedHandler TracksImported;
        event EventHandler ImportingTracks;
    }
}
