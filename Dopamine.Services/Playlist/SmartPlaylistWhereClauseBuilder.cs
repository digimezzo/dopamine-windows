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
                case SmartPlaylistDecoder.MatchAny:
                    whereOperator = "OR";
                    break;
                case SmartPlaylistDecoder.MatchAll:
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
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldArtist, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"Artists LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"Artists NOT LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorContains))
                {
                    whereSubClause = $"Artists LIKE '%{rule.Value}%'";
                }
            }

            // AlbumArtist
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldAlbumArtist, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"AlbumArtists NOT LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorContains))
                {
                    whereSubClause = $"AlbumArtists LIKE '%{rule.Value}%'";
                }
            }

            // Genre
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldGenre, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"Genres LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"Genres NOT LIKE '%{FormatUtils.DelimitValue(rule.Value)}%'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorContains))
                {
                    whereSubClause = $"Genres LIKE '%{rule.Value}%'";
                }
            }

            // Title
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldTitle, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"TrackTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"TrackTitle <> '{rule.Value}'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorContains))
                {
                    whereSubClause = $"TrackTitle LIKE '%{rule.Value}%'";
                }
            }

            // AlbumTitle
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldAlbumTitle, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"AlbumTitle = '{rule.Value}'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"AlbumTitle <> '{rule.Value}'";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorContains))
                {
                    whereSubClause = $"AlbumTitle LIKE '%{rule.Value}%'";
                }
            }

            // BitRate
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldBitrate, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"BitRate = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"BitRate <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"BitRate > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"BitRate < {rule.Value}";
                }
            }

            // TrackNumber
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldTrackNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"TrackNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"TrackNumber <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"TrackNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"TrackNumber < {rule.Value}";
                }
            }

            // TrackCount
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldTrackCount, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"TrackCount = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"TrackCount <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"TrackCount > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"TrackCount < {rule.Value}";
                }
            }

            // DiscNumber
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldDiscNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"DiscNumber = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"DiscNumber <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"DiscNumber > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"DiscNumber < {rule.Value}";
                }
            }

            // DiscCount
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldDiscCount, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"DiscCount = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"DiscCount <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"DiscCount > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"DiscCount < {rule.Value}";
                }
            }

            // Year
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldYear, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"Year = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"Year <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"Year > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"Year < {rule.Value}";
                }
            }

            // Rating
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldRating, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"Rating = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"Rating <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"Rating > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"Rating < {rule.Value}";
                }
            }

            // Love
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldLove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"Love = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"Love <> {rule.Value}";
                }
            }

            // PlayCount
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldPlayCount, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"PlayCount = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"PlayCount <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"PlayCount > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"PlayCount < {rule.Value}";
                }
            }

            // SkipCount
            if (rule.Field.Equals(SmartPlaylistDecoder.FieldSkipCount, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIs))
                {
                    whereSubClause = $"SkipCount = {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorIsNot))
                {
                    whereSubClause = $"SkipCount <> {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorGreaterThan))
                {
                    whereSubClause = $"SkipCount > {rule.Value}";
                }
                else if (rule.Operator.Equals(SmartPlaylistDecoder.OperatorLessThan))
                {
                    whereSubClause = $"SkipCount < {rule.Value}";
                }
            }

            return whereSubClause;
        }
    }
}
