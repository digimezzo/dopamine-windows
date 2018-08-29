using Digimezzo.Utilities.Helpers;
using Dopamine.Core.Base;
using System.Collections.Generic;

namespace Dopamine.Core.IO
{
    public class DecodeSmartPlaylistResult
    {
        public OperationResult DecodeResult { get; set; }

        public string PlaylistName { get; set; }

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
        public DecodeSmartPlaylistResult DecodePlaylist(string fileName)
        {
            OperationResult decodeResult = new OperationResult { Result = false };

            if (!System.IO.Path.GetExtension(fileName.ToLower()).Equals(FileFormats.DSPL))
            {
                return new DecodeSmartPlaylistResult { DecodeResult = new OperationResult { Result = false } };
            }

            string playlistName = string.Empty;
            string match = string.Empty;
            string order = string.Empty;
            string limit = string.Empty;
            IList<Rule> rules = new List<Rule>();

            // TODO

            return new DecodeSmartPlaylistResult
            {
                DecodeResult = decodeResult,
                PlaylistName = playlistName,
                Match = match,
                Order = order,
                Rules = rules
            };
        }
    }
}
