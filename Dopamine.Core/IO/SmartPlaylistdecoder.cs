using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.Log;
using Dopamine.Core.Base;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Dopamine.Core.IO
{
    public class DecodeSmartPlaylistResult
    {
        public OperationResult DecodeResult { get; set; }

        public string PlaylistName { get; set; }

        public string Match { get; set; }

        public string Order { get; set; }

        public string Limit { get; set; }

        public IList<Rule> Rules { get; set; }
    }

    public class Rule
    {
        public string Field { get; private set; }

        public string Operator { get; private set; }

        public string Value { get; private set; }

        public Rule(string field, string @operator, string value)
        {
            this.Field = field;
            this.Operator = @operator;
            this.Value = value;
        }
    }

    public class SmartPlaylistDecoder
    {
        public DecodeSmartPlaylistResult DecodePlaylist(string fileName)
        {
            if (!System.IO.Path.GetExtension(fileName.ToLower()).Equals(FileFormats.DSPL))
            {
                return new DecodeSmartPlaylistResult { DecodeResult = new OperationResult { Result = false } };
            }

            OperationResult decodeResult = new OperationResult { Result = true };

            string playlistName = string.Empty;
            string match = string.Empty;
            string order = string.Empty;
            string limit = string.Empty;
            IList<Rule> rules = new List<Rule>();

            try
            {
                XDocument xdoc = XDocument.Load(fileName);

                // Name
                XElement nameElement = (from t in xdoc.Element("smartplaylist").Elements("name")
                                        select t).FirstOrDefault();

                playlistName = nameElement != null ? nameElement.Value : string.Empty;

                // Match
                XElement matchElement = (from t in xdoc.Element("smartplaylist").Elements("match")
                                         select t).FirstOrDefault();

                match = matchElement != null ? matchElement.Value : string.Empty;

                // Order
                XElement orderElement = (from t in xdoc.Element("smartplaylist").Elements("order")
                                         select t).FirstOrDefault();

                order = orderElement != null ? orderElement.Value : string.Empty;

                // Limit
                XElement limitElement = (from t in xdoc.Element("smartplaylist").Elements("limit")
                                         select t).FirstOrDefault();

                limit = limitElement != null ? limitElement.Value : string.Empty;

                // Rules
                IList<XElement> ruleElements = (from t in xdoc.Element("smartplaylist").Elements("rule")
                                                select t).ToList();

                if (ruleElements == null || ruleElements.Count == 0)
                {
                    throw new Exception($"{nameof(ruleElements)} is null or contains no elements");
                }

                foreach (XElement ruleElement in ruleElements)
                {
                    rules.Add(new Rule(ruleElement.Attribute("field").Value, ruleElement.Attribute("operator").Value, ruleElement.Value));
                }
            }
            catch (Exception ex)
            {
                LogClient.Error($"Could not decode smart playlist '{fileName}'. Exception: {ex.Message}");
                decodeResult.Result = false;
            }

            return new DecodeSmartPlaylistResult
            {
                DecodeResult = decodeResult,
                PlaylistName = playlistName,
                Match = match,
                Order = order,
                Limit = limit,
                Rules = rules
            };
        }

        public string GetWhereOperator(string playlistMatch)
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

        public string GetLimit(string playlistLimit)
        {
            string limit = string.Empty;

            long parsedLong = 0;

            if (long.TryParse(playlistLimit, out parsedLong) && parsedLong > 0)
            {
                limit = $"LIMIT {parsedLong}";
            }

            return limit;
        }

        public string GetOrder(string playlistOrder)
        {
            string sqlOrder = string.Empty;

            switch (playlistOrder)
            {
                case "descending":
                    sqlOrder = "DESC";
                    break;
                case "ascending":
                default:
                    sqlOrder = "ASC";
                    break;
            }

            return sqlOrder;
        }

        public string GetWhereClausePart(Rule rule)
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
