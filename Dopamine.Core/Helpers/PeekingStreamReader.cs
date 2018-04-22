using System.Collections.Generic;
using System.IO;

namespace Dopamine.Core.Helpers
{
    public class PeekingStreamReader : StreamReader
    {
        private Queue<string> peeks;

        public PeekingStreamReader(Stream stream) : base(stream)
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

        public string PeekReadLine()
        {
            string nextLine = ReadLine();
            this.peeks.Enqueue(nextLine);
            return nextLine;
        }
    }
}
