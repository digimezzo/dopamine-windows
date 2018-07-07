using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dopamine.Data
{
    public sealed class DataUtils
    {
        public static string EscapeQuotes(string source)
        {
            return source.Replace("'", "''");
        }

        public static string CreateInClause(string columnName, IList<string> clauseItems)
        {
            string commaSeparatedItems = string.Join(",", clauseItems.Select((item) => "'" + EscapeQuotes(item) + "'").ToArray());

            return $"{columnName} IN ({commaSeparatedItems})";
        }

        public static string CreateOrLikeClause(string columnName, IList<string> clauseItems, string delimiter = "")
        {
            var sb = new StringBuilder();

            sb.AppendLine("(");

            var orClauses = new List<string>();

            foreach (string clauseItem in clauseItems)
            {
                if (string.IsNullOrEmpty(clauseItem))
                {
                    orClauses.Add($@"{columnName} IS NULL OR {columnName}=''");
                }
                else
                {
                    orClauses.Add($@"LOWER({columnName}) LIKE '%{delimiter}{clauseItem.Replace("'", "''").ToLower()}{delimiter}%'");
                }
            }

            sb.AppendLine(string.Join(" OR ", orClauses.ToArray()));
            sb.AppendLine(")");

            return sb.ToString();
        }
    }
}
