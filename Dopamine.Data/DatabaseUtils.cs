using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Data
{
    public sealed class DatabaseUtils
    {
        public static string ToQueryList(IList<long> list)
        {
            return string.Join(",", list.ToArray());
        }

        public static string ToQueryList(IList<string> list)
        {
            var str = string.Join(",", list.Select((item) => "'" + item.Replace("'", "''") + "'").ToArray());
            return str;
        }
    }
}
