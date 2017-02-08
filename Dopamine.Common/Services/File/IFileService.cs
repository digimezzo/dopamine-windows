using Dopamine.Common.Database;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.File
{
   public delegate void TracksImportedHandler(List<PlayableTrack> tracks);

    [ServiceContract(Namespace = "http://Dopamine.FileService")]
    public interface IFileService
    {
        [OperationContract()]
        void ProcessArguments(string[] iArgs);

        Task<PlayableTrack> CreateTrackAsync(string path);

        event TracksImportedHandler TracksImported;
    }
}
