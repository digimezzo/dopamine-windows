namespace Dopamine.Core.Audio
{
    public class PlayerFactory : IPlayerFactory
    {
        public IPlayer Create(bool supportsWasapi)
        {
            IPlayer player = CSCorePlayer.Instance;
            player.SupportsWindowsMediaFoundation = supportsWasapi;

            return player;
        }
    }
}
