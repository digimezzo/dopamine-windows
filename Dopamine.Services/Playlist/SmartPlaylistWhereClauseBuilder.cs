using Dopamine.Core.IO;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Playlist
{
    public class SmartPlaylistWhereClauseBuilder
    {
        private DecodeSmartPlaylistResult result;

        public SmartPlaylistWhereClauseBuilder(DecodeSmartPlaylistResult result)
        {
            this.result = result;
        }

        public string GetWhereClause()
        {
            string sqlWhereOperator = this.GetWhereOperator(result.Match);

            IList<string> whereClauseParts = new List<string>();

            foreach (SmartPlaylistRule rule in result.Rules)
            {
                string whereClausePart = this.GetWhereClausePart(rule);

                if (!string.IsNullOrWhiteSpace(whereClausePart))
                {
                    whereClauseParts.Add(whereClausePart);
                }
            }

            string whereClause = string.Join($" {sqlWhereOperator} ", whereClauseParts.ToArray());
            whereClause = $"({whereClause})";

            return whereClause;
        }

        private string GetWhereOperator(string playlistMatch)
        {
            string whereOperator = string.Empty;

            switch (playlistMatch)
            {
                case "any":
                    whereOperator = "OR";
                    break;
                case "all":
                default:
                    whereOperator = "AND";
                    break;
            }

            return whereOperator;
        }

        private string GetWhereClausePart(SmartPlaylistRule rule)
        {
            string whereSubClause = string.Empty;

            // Artist
            if (rule.Field.Equals("artist", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Artists LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"Artists LIKE '%{rule.Value}%'";
                }
            }

            // AlbumArtist
            if (rule.Field.Equals("albumartist", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{rule.Value}%'";
                }
            }

            // Genre
            if (rule.Field.Equals("genre", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Genres LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"Genres LIKE '%{rule.Value}%'";
                }
            }

            // Title
            if (rule.Field.Equals("title", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"TrackTitle LIKE '%{rule.Value}%'";
                }
            }

            // AlbumTitle
            if (rule.Field.Equals("albumtitle", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"AlbumTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals("contains"))
                {
                    whereSubClause = $"AlbumTitle LIKE '%{rule.Value}%'";
                }
            }

            // BitRate
            if (rule.Field.Equals("bitrate", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"BitRate = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"BitRate > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"BitRate < {rule.Value}";
                }
            }

            // TrackNumber
            if (rule.Field.Equals("tracknumber", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"TrackNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"TrackNumber < {rule.Value}";
                }
            }

            // TrackCount
            if (rule.Field.Equals("trackcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"TrackCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"TrackCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"TrackCount < {rule.Value}";
                }
            }

            // DiscNumber
            if (rule.Field.Equals("discnumber", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"DiscNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"DiscNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"DiscNumber < {rule.Value}";
                }
            }

            // DiscCount
            if (rule.Field.Equals("disccount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"DiscCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"DiscCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"DiscCount < {rule.Value}";
                }
            }

            // Year
            if (rule.Field.Equals("year", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Year = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Year > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Year < {rule.Value}";
                }
            }

            // Rating
            if (rule.Field.Equals("rating", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Rating = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Rating > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Rating < {rule.Value}";
                }
            }

            // Love
            if (rule.Field.Equals("love", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"Love = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"Love > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"Love < {rule.Value}";
                }
            }

            // PlayCount
            if (rule.Field.Equals("playcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"PlayCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"PlayCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"PlayCount < {rule.Value}";
                }
            }

            // SkipCount
            if (rule.Field.Equals("skipcount", StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals("is"))
                {
                    whereSubClause = $"SkipCount = {rule.Value}";
                }
                else if (rule.Operator.Equals("greaterthan"))
                {
                    whereSubClause = $"SkipCount > {rule.Value}";
                }
                else if (rule.Operator.Equals("lessthan"))
                {
                    whereSubClause = $"SkipCount < {rule.Value}";
                }
            }

            return whereSubClause;
        }
    }
}
