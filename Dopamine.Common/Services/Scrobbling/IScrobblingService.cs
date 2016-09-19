using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Scrobbling
{
    public interface IScrobblingService
    {
        bool IsSignedIn { get; }
        bool IsEnabled { get; set; }
        string Username { get; set; }
        string Password { get; set; }

        event Action<bool> SignInStateChanged;

        Task SignIn();
        void SignOut();
    }
}
