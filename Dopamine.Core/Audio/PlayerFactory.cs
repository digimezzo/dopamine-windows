namespace Dopamine.Core.Audio
{
    public class PlayerFactory : IPlayerFactory
    {
        public IPlayer Create()
        {
            return CSCorePlayer.Instance;
        }
    }
}
