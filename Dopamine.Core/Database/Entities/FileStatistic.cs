using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Core.Database.Entities
{
    public class FileStatistic
    {
        #region Properties
        [Key()]
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
