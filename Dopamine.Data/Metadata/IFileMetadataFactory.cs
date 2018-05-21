namespace Dopamine.Data.Metadata
{
    public interface IFileMetadataFactory
    {
        IFileMetadata Create(string path);
    }
}
