using SQLite;

namespace Dopamine.Common.Database.Entities
{
    public class Configuration
    {
        [PrimaryKey(), AutoIncrement()]
        public long ConfigurationID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
