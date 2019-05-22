using Digimezzo.Foundation.Core.Helpers;
using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Base;
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

        // public string Order { get; set; } // TODO: order by

        public SmartPlaylistLimit Limit { get; set; }

        public IList<SmartPlaylistRule> Rules { get; set; }
    }

    public class SmartPlaylistRule
    {
        public string Field { get; private set; }

        public string Operator { get; private set; }

        public string Value { get; private set; }

        public SmartPlaylistRule(string field, string @operator, string value)
        {
            this.Field = field;
            this.Operator = @operator;
            this.Value = value;
        }
    }

    public enum SmartPlaylistLimitType
    {
        Songs = 1,
        GigaBytes = 2,
        MegaBytes = 3,
        Minutes = 4
    }

    public class SmartPlaylistLimit
    {
        public SmartPlaylistLimitType Type { get; private set; }

        public int Value { get; private set; }

        public SmartPlaylistLimit(SmartPlaylistLimitType type, int value)
        {
            this.Type = type;
            this.Value = value;
        }

        public static string TypeToString(SmartPlaylistLimitType type)
        {
            switch (type)
            {
                case SmartPlaylistLimitType.Songs:
                    return SmartPlaylistDecoder.XmlLimitTypeSongs;
                case SmartPlaylistLimitType.GigaBytes:
                    return SmartPlaylistDecoder.XmlLimitTypeGigaBytes;
                case SmartPlaylistLimitType.MegaBytes:
                    return SmartPlaylistDecoder.XmlLimitTypeMegaBytes;
                case SmartPlaylistLimitType.Minutes:
                    return SmartPlaylistDecoder.XmlLimitTypeMinutes;
                default:
                    return SmartPlaylistDecoder.XmlLimitTypeSongs;
            }
        }

        public static SmartPlaylistLimitType StringToType(string typeString)
        {
            switch (typeString)
            {
                case SmartPlaylistDecoder.XmlLimitTypeSongs:
                    return SmartPlaylistLimitType.Songs;
                case SmartPlaylistDecoder.XmlLimitTypeGigaBytes:
                    return SmartPlaylistLimitType.GigaBytes;
                case SmartPlaylistDecoder.XmlLimitTypeMegaBytes:
                    return SmartPlaylistLimitType.MegaBytes;
                case SmartPlaylistDecoder.XmlLimitTypeMinutes:
                    return SmartPlaylistLimitType.Minutes;
                default:
                    return SmartPlaylistLimitType.Songs;
            }
        }
    }

    public class SmartPlaylistDecoder
    {
        public const string FieldArtist = "artist";
        public const string FieldAlbumArtist = "albumartist";
        public const string FieldGenre = "genre";
        public const string FieldTitle = "title";
        public const string FieldAlbumTitle = "albumtitle";
        public const string FieldBitrate = "bitrate";
        public const string FieldTrackNumber = "tracknumber";
        public const string FieldTrackCount = "trackcount";
        public const string FieldDiscNumber = "discnumber";
        public const string FieldDiscCount = "disccount";
        public const string FieldYear = "year";
        public const string FieldRating = "rating";
        public const string FieldLove = "love";
        public const string FieldPlayCount = "playcount";
        public const string FieldSkipCount = "skipcount";

        public const string OperatorIs = "is";
        public const string OperatorIsNot = "isnot";
        public const string OperatorContains = "contains";
        public const string OperatorGreaterThan = "greaterthan";
        public const string OperatorLessThan = "lessthan";

        public const string MatchAny = "any";
        public const string MatchAll = "all";

        public const string XmlElementSmartPlaylist = "smartplaylist";
        public const string XmlElementName = "name";
        public const string XmlElementMatch = "match";
        public const string XmlElementOrder = "order";
        public const string XmlElementLimit = "limit";
        public const string XmlElementRule = "rule";

        public const string XmlAttributeField = "field";
        public const string XmlAttributeOperator = "operator";
        public const string XmlAttributeType = "type";

        public const string XmlLimitTypeSongs = "songs";
        public const string XmlLimitTypeGigaBytes = "GB";
        public const string XmlLimitTypeMegaBytes = "MB";
        public const string XmlLimitTypeMinutes = "Minutes";

        public XDocument EncodeSmartPlaylist(string name, bool matchAnyRule, SmartPlaylistLimit limit, IList<SmartPlaylistRule> rules)
        {
            XDocument doc =
              new XDocument(
                new XElement(XmlElementSmartPlaylist,
                  new XElement(XmlElementName) { Value = name },
                  new XElement(XmlElementMatch) { Value = matchAnyRule ? MatchAny : MatchAll },
                  new XElement(XmlElementLimit, new XAttribute(XmlAttributeType, SmartPlaylistLimit.TypeToString(limit.Type))) { Value = limit.Value.ToString() },
                    rules.Select(x => new XElement(XmlElementRule,
                    new XAttribute(XmlAttributeField, x.Field),
                    new XAttribute(XmlAttributeOperator, x.Operator))
                    { Value = x.Value })
                )
              );

            return doc;
        }

        public DecodeSmartPlaylistResult DecodePlaylist(string fileName)
        {
            if (!System.IO.Path.GetExtension(fileName.ToLower()).Equals(FileFormats.DSPL))
            {
                return new DecodeSmartPlaylistResult { DecodeResult = new OperationResult { Result = false } };
            }

            OperationResult decodeResult = new OperationResult { Result = true };

            string playlistName = string.Empty;
            string match = string.Empty;
            // string order = string.Empty; // TODO: order by
            SmartPlaylistLimit limit = new SmartPlaylistLimit(SmartPlaylistLimitType.Songs, 0);
            IList<SmartPlaylistRule> rules = new List<SmartPlaylistRule>();

            try
            {
                XDocument xdoc = XDocument.Load(fileName);

                // Name
                XElement nameElement = (from t in xdoc.Element(XmlElementSmartPlaylist).Elements(XmlElementName)
                                        select t).FirstOrDefault();

                playlistName = nameElement != null ? nameElement.Value : string.Empty;

                // Match
                XElement matchElement = (from t in xdoc.Element(XmlElementSmartPlaylist).Elements(XmlElementMatch)
                                         select t).FirstOrDefault();

                match = matchElement != null ? matchElement.Value : string.Empty;

                // Order
                //XElement orderElement = (from t in xdoc.Element(XmlElementSmartPlaylist).Elements(XmlElementOrder)
                //                         select t).FirstOrDefault();

                // order = orderElement != null ? orderElement.Value : string.Empty;

                // Limit
                XElement limitElement = (from t in xdoc.Element(XmlElementSmartPlaylist).Elements(XmlElementLimit)
                                         select t).FirstOrDefault();

                if (limitElement != null && !string.IsNullOrEmpty(limitElement.Attribute(XmlAttributeType).Value))
                {
                    int limitValue = 0;

                    if (limitElement.Attribute(XmlAttributeType) != null && int.TryParse(limitElement.Value, out limitValue))
                    {
                        limit = new SmartPlaylistLimit(SmartPlaylistLimit.StringToType(limitElement.Attribute(XmlAttributeType).Value), limitValue);
                    }
                }

                // Rules
                IList<XElement> ruleElements = (from t in xdoc.Element(XmlElementSmartPlaylist).Elements(XmlElementRule)
                                                select t).ToList();

                if (ruleElements == null || ruleElements.Count == 0)
                {
                    throw new Exception($"{nameof(ruleElements)} is null or contains no elements");
                }

                foreach (XElement ruleElement in ruleElements)
                {
                    rules.Add(new SmartPlaylistRule(ruleElement.Attribute(XmlAttributeField).Value, ruleElement.Attribute(XmlAttributeOperator).Value, ruleElement.Value));
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
                Limit = limit,
                Rules = rules
            };
        }
    }
}
