using SQLite.Net.Attributes;

namespace Dopamine.Core.Database.Entities
{
    public class FileStatistic
    {
        #region Properties
        [PrimaryKey()]
        public long FileStatisticID { get; set; }
        public string Path { get; set; }
        public long? Rating { get; set; }
        #endregion

        #region Override
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Path.Equals(((FileStatistic)obj).Path);
        }

        public override int GetHashCode()
        {
            return new { this.Path }.GetHashCode();
        }
        #endregion
    }
}
