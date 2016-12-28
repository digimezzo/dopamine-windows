namespace Dopamine.Common.Audio
{
    public interface IPlayerFactory
    {
        IPlayer Create(string path);
    }
}