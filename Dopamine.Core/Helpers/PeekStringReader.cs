using System.Collections.Generic;
using System.IO;

namespace Dopamine.Core.Helpers
{
    public class PeekStringReader : StringReader
    {
        private Queue<string> peeks;

        public PeekStringReader(string text) : base(text)
        {
            this.peeks = new Queue<string>();
        }

        public override string ReadLine()
        {
            if (this.peeks.Count > 0)
            {
                string nextLine = this.peeks.Dequeue();

                return nextLine;
            }

            return base.ReadLine();
        }

        public string PeekLine()
        {
            string nextLine = this.ReadLine();
            this.peeks.Enqueue(nextLine);

            return nextLine;
        }
    }
}
