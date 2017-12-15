using Dopamine.Data;
using System;
using System.Threading.Tasks;

namespace Dopamine.Services.Scrobbling
{
    public interface IScrobblingService
    {
        SignInState SignInState { get; set; }
        string Username { get; set; }
        string Password { get; set; }

        event Action<SignInState> SignInStateChanged;

        Task SignIn();
        void SignOut();

        Task<bool> SendTrackLoveAsync(PlayableTrack track, bool love);
    }
}
