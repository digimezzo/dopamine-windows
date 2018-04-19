namespace Dopamine.Core.Audio
{
    public class PlayerFactory : IPlayerFactory
    {
        public IPlayer Create(bool supportsWindowsMediaFoundation)
        {
            IPlayer player = CSCorePlayer.Instance;
            player.SupportsWindowsMediaFoundation = supportsWindowsMediaFoundation;

            return player;
        }
    }
}
