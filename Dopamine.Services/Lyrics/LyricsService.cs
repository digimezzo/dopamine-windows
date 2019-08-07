using Dopamine.Core.Helpers;
using Dopamine.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dopamine.Core.Api.Lyrics;

namespace Dopamine.Services.Lyrics
{
    public class LyricsService : ILyricsService
    {
        private int GetNumberOfFollowingEmptyLines(ref PeekStringReader reader)
        {
            int numberOfEmptyLines = 0;

            string peekedLine = reader.PeekLine();

            while (peekedLine != null && peekedLine.Length == 0)
            {
                // The next line is an empty line
                numberOfEmptyLines++;
                reader.ReadLine();
                peekedLine = reader.PeekLine();
            }

            return numberOfEmptyLines;
        }

        private void AddEmptyLines(int numberOfEmptyLines, List<LyricsLineViewModel> lines, TimeSpan span)
        {
            for (int i = 0; i < numberOfEmptyLines; i++)
            {
                if (span.Equals(TimeSpan.MinValue))
                {
                    lines.Add(new LyricsLineViewModel(string.Empty));
                }
                else
                {
                    lines.Add(new LyricsLineViewModel(span, string.Empty));
                }  
            }
        }

        public IList<LyricsLineViewModel> ParseLyrics(Core.Api.Lyrics.Lyrics lyrics)
        {
            var linesWithTimestamps = new List<LyricsLineViewModel>();
            var linesWithoutTimestamps = new List<LyricsLineViewModel>();

            var reader = new PeekStringReader(lyrics.Text);

            string line;

            while (true)
            {
                // Process 1 line
                line = reader.ReadLine();

                if (line == null)
                {
                    // No line found, we reached the end. Exit while loop.
                    break;
                }

                // Ignore empty lines
                if (line.Length == 0)
                {
                    // Process the next line.
                    continue;
                }

                // Ignore lines with tags
                MatchCollection tagMatches = Regex.Matches(line, @"\[[a-z]+?:.*?\]");

                if (tagMatches.Count > 0)
                {
                    // This is a tag: ignore this line and process the next line.
                    continue;
                }

                // Check if the line has characters and is enclosed in brackets (starts with [ and ends with ]).
                if (!(line.StartsWith("[") && line.LastIndexOf(']') > 0))
                {
                    // This line is not enclosed in brackets, so it cannot have timestamps.
                    linesWithoutTimestamps.Add(new LyricsLineViewModel(line));
                    int numberOfEmptyLines = this.GetNumberOfFollowingEmptyLines(ref reader);

                    // Add empty lines
                    this.AddEmptyLines(numberOfEmptyLines, linesWithoutTimestamps, TimeSpan.MinValue);

                    // Process the next line
                    continue;
                }

                // Get all substrings between square brackets for this line
                MatchCollection ms = Regex.Matches(line, @"\[.*?\]");
                var spans = new List<TimeSpan>();
                bool couldParseAllTimestamps = true;

                // Loop through all matches
                foreach (Match m in ms)
                {
                    var time = TimeSpan.Zero;
                    string subString = m.Value.Trim('[', ']');

                    if (FormatUtils.ParseLyricsTime(subString, out time))
                    {
                        spans.Add(time);
                    }
                    else
                    {
                        couldParseAllTimestamps = false;
                    }
                }

                // Check if all timestamps could be parsed
                if (couldParseAllTimestamps)
                {
                    int startIndex = line.LastIndexOf(']') + 1;
                    int numberOfEmptyLines = this.GetNumberOfFollowingEmptyLines(ref reader);

                    foreach (TimeSpan span in spans)
                    {
                        linesWithTimestamps.Add(new LyricsLineViewModel(span, line.Substring(startIndex)));

                        // Add empty lines
                        this.AddEmptyLines(numberOfEmptyLines, linesWithTimestamps, span);
                    }
                }
                else
                {
                    // The line has mistakes. Consider it as a line without timestamps.
                    linesWithoutTimestamps.Add(new LyricsLineViewModel(line));
                    int numberOfEmptyLines = this.GetNumberOfFollowingEmptyLines(ref reader);

                    // Add empty lines
                    this.AddEmptyLines(numberOfEmptyLines, linesWithoutTimestamps, TimeSpan.MinValue);
                }
            }

            // Order the time stamped lines
            linesWithTimestamps = new List<LyricsLineViewModel>(linesWithTimestamps.OrderBy(p => p.Time));

            // Merge both collections, lines with timestamps first.
            linesWithTimestamps.AddRange(linesWithoutTimestamps);

            return linesWithTimestamps;
        }
    }
}
