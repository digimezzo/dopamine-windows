using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dopamine.Core.Helpers
{
    public class OperationResult
    {
        #region Variabels
        private List<string> messages;
        #endregion

        #region Properties
        public bool Result { get; set; }
        #endregion

        #region Construction

        public OperationResult()
        {
            this.messages = new List<string>();
        }
        #endregion

        #region Public
        public void AddMessage(string iMessage)
        {
            this.messages.Add(iMessage);
        }

        public string GetFirstMessage()
        {
            if (this.messages.Count > 0)
            {
                return this.messages.First();
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetMessages()
        {

            StringBuilder sb = new StringBuilder();

            foreach (string item in this.messages)
            {
                sb.AppendLine(item + Environment.NewLine);
            }

            return sb.ToString();
        }
        #endregion
    }
}
