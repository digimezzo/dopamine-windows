using System.Collections.Generic;
using System.IO;

namespace Dopamine.Core.Helpers
{
    public class PeekingStringReader
    {
        private readonly StringReader reader;
        private readonly Queue<string> readAheadQueue = new Queue<string>();

        public PeekingStringReader(StringReader reader)
        {
            this.reader = reader;

        }

        public string ReadLine()
        {
            if (readAheadQueue.Count > 0)
            {
                return readAheadQueue.Dequeue();
            }

            return reader.ReadLine();
        }

        public string PeekLine()
        {
            if (readAheadQueue.Count > 0)
            {
                return readAheadQueue.Peek();
            }

            string line = reader.ReadLine();

            if (line != null)
            {
                readAheadQueue.Enqueue(line);
            }

            return line;
        }
    }
}
