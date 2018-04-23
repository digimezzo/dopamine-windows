using Dopamine.Core.Api.Lyrics;
using Dopamine.Core.Helpers;
using Dopamine.Core.Utils;
using Dopamine.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dopamine.Presentation.Utils
{
    public static class LyricsUtils
    {
        private static int GetNumberOfFollowingEmptyLines(ref PeekStringReader reader)
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

        private static void AddEmptyLines(int numberOfEmptyLines, List<LyricsLineViewModel> lines, TimeSpan span)
        {
            for (int i = 0; i < numberOfEmptyLines; i++)
            {
                lines.Add(new LyricsLineViewModel(span, string.Empty));
            }
        }

        public static IList<LyricsLineViewModel> ParseLyrics(Lyrics lyrics)
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
                    int numberOfEmptyLines = GetNumberOfFollowingEmptyLines(ref reader);

                    // Add empty lines
                    AddEmptyLines(numberOfEmptyLines, linesWithoutTimestamps, TimeSpan.Zero);

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
                    int numberOfEmptyLines = GetNumberOfFollowingEmptyLines(ref reader);

                    foreach (TimeSpan span in spans)
                    {
                        linesWithTimestamps.Add(new LyricsLineViewModel(span, line.Substring(startIndex)));

                        // Add empty lines
                        AddEmptyLines(numberOfEmptyLines, linesWithTimestamps, span);
                    }
                }
                else
                {
                    // The line has mistakes. Consider it as a line without timestamps.
                    linesWithoutTimestamps.Add(new LyricsLineViewModel(line));
                    int numberOfEmptyLines = GetNumberOfFollowingEmptyLines(ref reader);

                    // Add empty lines
                    AddEmptyLines(numberOfEmptyLines, linesWithoutTimestamps, TimeSpan.Zero);
                }
            }

            // Order the time stamped lines
            linesWithTimestamps = new List<LyricsLineViewModel>(linesWithTimestamps.OrderBy(p => p.Time).ThenByDescending(p => p.Text));

            // Merge both collections, lines with timestamps first.
            linesWithTimestamps.AddRange(linesWithoutTimestamps);

            return linesWithTimestamps;
        }
    }
}
