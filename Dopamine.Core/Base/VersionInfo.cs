using System;
using Dopamine.Core.Base;

namespace Dopamine.Core.Base
{
    public class VersionInfo
    {
        public Version Version { get; set; }
        public Configuration Configuration { get; set; }

        public string VersionTag
        {
            get { return this.Configuration == Configuration.Release ? "Release" : "Preview"; }
        }


        public VersionInfo()
        {
            this.Version = new Version(0, 0, 0, 0);
            this.Configuration = Configuration.Debug;
        }

        public bool IsOlder(VersionInfo referenceVersion)
        {
            return this.Version < referenceVersion.Version;
        }
    }
}
