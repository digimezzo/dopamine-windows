using Digimezzo.Utilities.Helpers;
using System.Collections.Generic;

namespace Dopamine.Core.IO
{
    public class DecodeSmartPlaylistResult
    {
        public OperationResult DecodeResult { get; set; }

        public string Name { get; set; }

        public string Match { get; set; }

        public string Order { get; set; }

        public long Limit { get; set; }

        public IList<Rule> Rules { get; set; }
    }

    public class Rule
    {
        public string Field { get; set; }

        public string Operator { get; set; }

        public string Value { get; set; }
    }

    public class SmartPlaylistdecoder
    {
    }
}
