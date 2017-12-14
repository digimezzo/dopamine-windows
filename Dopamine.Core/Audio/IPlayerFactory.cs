namespace Dopamine.Core.Audio
{
    public interface IPlayerFactory
    {
        IPlayer Create(string path);
    }
}