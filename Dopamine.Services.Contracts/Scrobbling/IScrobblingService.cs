using Dopamine.Data.Contracts.Entities;
using System;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.Scrobbling
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
