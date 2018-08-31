using Digimezzo.Utilities.Helpers;
using Digimezzo.Utilities.Log;
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

    public class SmartPlaylistdecoder
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
    }
}
