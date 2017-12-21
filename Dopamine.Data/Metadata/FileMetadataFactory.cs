using Dopamine.Data.Contracts.Metadata;

namespace Dopamine.Data.Metadata
{
    public class FileMetadataFactory : IFileMetadataFactory
    {
        public IFileMetadata Create(string path)
        {
            return new FileMetadata(path);
        }
    }
}
