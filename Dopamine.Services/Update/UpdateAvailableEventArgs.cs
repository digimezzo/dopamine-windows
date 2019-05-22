using Digimezzo.Foundation.Core.Packaging;
using System;

namespace Dopamine.Services.Update
{
    public class UpdateAvailableEventArgs : EventArgs
    {
        public Package UpdatePackage { get; set; }
        public string UpdatePackageLocation { get; set; }
    }
}
