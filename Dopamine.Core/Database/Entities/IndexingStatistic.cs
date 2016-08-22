using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class IndexingStatistic
    {
        #region Properties
        [PrimaryKey(), AutoIncrement()]
        public long IndexingStatisticID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        #endregion
    }
}
