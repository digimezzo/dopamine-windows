using System.Collections.Generic;

namespace Dopamine.Common.Services.I18n
{
    public class Language
    {
        #region Variables
        private string code;
        private string name;
        private string author;
        private Dictionary<string, string> texts;
        #endregion

        #region Properties
        public string Code
        {
            get { return this.code; }
            set { this.code = value; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Author
        {
            get { return this.author; }
            set { this.author = value; }
        }

        public Dictionary<string, string> Texts
        {
            get { return this.texts; }
            set { this.texts = value; }
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

            return this.Code.Equals(((Language)obj).Code);
        }

        public override int GetHashCode()
        {
            return this.Code.GetHashCode();
        }
        #endregion
    }
}
