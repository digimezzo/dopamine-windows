using Dopamine.Core.Database;
using System.Collections.Generic;

namespace Dopamine.Common.Services.Playback
{
    public class AddToQueueResult
    {
        public bool IsSuccess { get; set; }
        public IList<string> AddedFiles { get; set; }
    }
}
