using Digimezzo.Utilities.Packaging;
using System;

namespace Dopamine.Common.Services.Update
{
    public class UpdateAvailableEventArgs : EventArgs
    {
        public Package UpdatePackage { get; set; }
        public string UpdatePackageLocation { get; set; }
    }
}
