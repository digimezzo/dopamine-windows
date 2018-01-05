using Digimezzo.Utilities.Packaging;
using System;

namespace Dopamine.Services.Contracts.Update
{
    public class UpdateAvailableEventArgs : EventArgs
    {
        public Package UpdatePackage { get; set; }
        public string UpdatePackageLocation { get; set; }
    }
}
