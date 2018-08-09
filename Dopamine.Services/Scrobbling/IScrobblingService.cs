using Dopamine.Services.Entities;
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

        Task SendTrackLoveAsync(TrackViewModel track, bool love);
    }
}
