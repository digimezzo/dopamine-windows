namespace Dopamine.Core.Settings
{
    public interface ISettingsClient
    {
        string SettingsFile { get; }
        bool IsSettingsUpgradeNeeded();
        void UpgradeSettings();
        void Write();
        void Set<T>(string settingNamespace, string settingName, T value);
        bool TryGet<T>(string settingNamespace, string settingName, ref T value);
        T Get<T>(string settingNamespace, string settingName);
    }
}
