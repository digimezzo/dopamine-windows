namespace Dopamine.Core.Audio
{
    public interface IPlayerFactory
    {
       IPlayer Create(bool hasMediaFoundationSupport);
    }
}