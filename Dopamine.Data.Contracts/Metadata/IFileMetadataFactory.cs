namespace Dopamine.Data.Contracts.Metadata
{
    public interface IFileMetadataFactory
    {
        IFileMetadata Create(string path);
    }
}
