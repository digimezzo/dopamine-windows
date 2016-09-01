using System;

namespace Dopamine.Common.Services.Provider
{
    public class SearchProvider
    {
        #region Properties
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Separator { get; set; }
        #endregion

        #region Construction
        public SearchProvider()
        {
            this.Id = Guid.NewGuid().ToString();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Id.Equals(((SearchProvider)obj).Id);
        }

        public override int GetHashCode()
        {
            return new { this.Id }.GetHashCode();
        }
        #endregion
    }
}
