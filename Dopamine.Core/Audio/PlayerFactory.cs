namespace Dopamine.Core.Audio
{
    public class PlayerFactory : IPlayerFactory
    {
        public IPlayer Create(bool hasMediaFoundationSupport)
        {
            return new NAudioPlayer(hasMediaFoundationSupport);
        }
    }
}
