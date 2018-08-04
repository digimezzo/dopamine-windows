namespace Dopamine.Core.Audio
{
    public class AudioDevice
    {
        public string DeviceId { get; }

        public string Name { get; }

        public override string ToString()
        {
            return this.Name;
        }

        public AudioDevice(string name, string deviceId)
        {
            this.Name = name;
            this.DeviceId = deviceId;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.DeviceId.Equals(((AudioDevice)obj).DeviceId);
        }

        public override int GetHashCode()
        {
            return new { this.DeviceId }.GetHashCode();
        }
    }
}
