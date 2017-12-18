using SQLite;

namespace Dopamine.Data.Contracts.Entities
{
    public class Configuration
    {
        [PrimaryKey(), AutoIncrement()]
        public long ConfigurationID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
