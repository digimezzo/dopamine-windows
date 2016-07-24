using System.ComponentModel.DataAnnotations;

namespace Dopamine.Core.Database.Entities
{
    public class Configuration
    {
        #region Properties
        [Key()]
        public long ConfigurationID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        #endregion
    }
}
