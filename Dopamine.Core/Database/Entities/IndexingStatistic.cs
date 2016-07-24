using System.ComponentModel.DataAnnotations;

namespace Dopamine.Core.Database.Entities
{
    public class IndexingStatistic
    {
        #region Properties
        [Key()]
        public long IndexingStatisticID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        #endregion
    }
}
