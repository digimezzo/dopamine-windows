using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Metadata
{
    public class LoveChangedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public bool Love { get; set; }
    }
}
