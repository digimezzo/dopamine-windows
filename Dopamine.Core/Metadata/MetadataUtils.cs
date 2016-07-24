using Dopamine.Core.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dopamine.Core.Metadata
{
    public class MetadataUtils
    {
        public static IEnumerable<string> PatchID3v23Enumeration(IEnumerable<string> valuesEnumer)
        {
            return JoinUnsplittableValues(valuesEnumer, Defaults.UnsplittableTagValues, '/');
        }

        public static IEnumerable<string> JoinUnsplittableValues(IEnumerable<string> valuesEnumer, ICollection<string> unsplittableValues, char separator)
        {
            if (valuesEnumer == null)
            {
                return null;
            }
            else
            {
                List<string> values = new List<string>(valuesEnumer);

                if ((values.Count == 0))
                    return null;

                foreach (string unsplittableValue in unsplittableValues)
                {
                    JoinUnsplittableValue(ref values, unsplittableValue, separator);
                }

                return values;
            }
        }

        public static int IndexOf<S, T>(IList<S> list, IList<T> subList, IEqualityComparer<S> comparer)
        {
            for (int i = 0; i <= list.Count - subList.Count; i++)
            {
                bool allEqual = true;

                for (int j = 0; j <= subList.Count - 1; j++)
                {
                    if (!comparer.Equals(list[i + j], (S)(object)subList[j]))
                    {
                        allEqual = false;
                        break;
                    }
                }

                if (allEqual) return i;
            }

            return -1;
        }

        public static string Join(string separator, IEnumerable values)
        {

            if (values == null) return string.Empty;

            IEnumerator enumer = values.GetEnumerator();
            if (!enumer.MoveNext()) return string.Empty;

            var result = new StringBuilder();

            while (true)
            {
                result.Append(enumer.Current.ToString());
                if (enumer.MoveNext())
                {
                    result.Append(separator);
                }
                else
                {
                    return result.ToString();
                }
            }
        }

        public static void JoinUnsplittableValue(ref List<string> valueList, string unsplittableValue, char separator)
        {
            IList<string> parts = unsplittableValue.Split(separator);

            int index = IndexOf<string, string>(valueList, parts, StringComparer.InvariantCultureIgnoreCase);

            if (index == -1)
                return;

            string[] origParts = new string[parts.Count];

            for (int i = 0; i <= parts.Count - 1; i++)
            {
                origParts[i] = valueList[index];
                valueList.RemoveAt(index);
            }

            valueList.Insert(index, string.Join(separator.ToString(), origParts));
        }


        public static string SanitizeTag(string str)
        {

            if (!string.IsNullOrEmpty(str))
            {
                return str.Trim();
            }
            else
            {
                return string.Empty;
            }
        }

        public static long SafeConvertToLong(string str)
        {

            long result = 0;
            Int64.TryParse(str, out result);
            return result;
        }

        public static byte Star2PopMRating(int rating)
        {

            // 5 stars = POPM 255
            // 4 stars = POPM 196
            // 3 stars = POPM 128
            // 2 stars = POPM 64
            // 1 stars = POPM 1
            // 0 stars = POPM 0

            switch (rating)
            {
                case 0:
                    return 0;
                case 1:
                    return 1;
                case 2:
                    return 64;
                case 3:
                    return 128;
                case 4:
                    return 196;
                case 5:
                    return 255;
                default:
                    // Should not happen
                    return 0;
            }
        }

        public static int PopM2StarRating(byte popMRating)
        {

            // 0 stars = POPM 0
            // 1 stars = POPM 1
            // 2 stars = POPM 64
            // 3 stars = POPM 128
            // 4 stars = POPM 196
            // 5 stars = POPM 255

            if (popMRating <= 0)
            {
                return 0;
            }
            else if (popMRating <= 1)
            {
                return 1;
            }
            else if (popMRating <= 64)
            {
                return 2;
            }
            else if (popMRating <= 128)
            {
                return 3;
            }
            else if (popMRating <= 196)
            {
                return 4;
            }
            else if (popMRating <= 255)
            {
                return 5;
            }
            else
            {
                return 0;
                // Should not happen
            }
        }

        public int GetRating(string path)
        {
            TagLib.File fi = TagLib.File.Create(path);
            TagLib.Tag tag = fi.GetTag(TagLib.TagTypes.Id3v2);

            TagLib.Id3v2.PopularimeterFrame popM = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, Defaults.PopMUser, true);

            return PopM2StarRating(popM.Rating);
        }
    }
}
