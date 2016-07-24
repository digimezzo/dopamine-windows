namespace Dopamine.Core.Helpers
{
    public class NameValue
    {
        #region Variables
        private string name;
        private int value;
        #endregion

        #region Properties
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public int Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return this.Name.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Value == ((NameValue)obj).Value;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
        #endregion
    }
}
