using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Core.Alex
{
    public static class SettingsClient
    {
        //public static SettingsClient Instance { get; }

        //public static event SettingChangedEventHandler SettingChanged;// => Digimezzo.Foundation.Core.Settings.Sett

        public static string ApplicationFolder()
        {
#if DEBUG //=== ALEX: Add this code in order to debug the program while using tha app
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().GetName().Name + "\\Debug");
#else
            return Digimezzo.Foundation.Core.Settings.SettingsClient.ApplicationFolder();
#endif
        }
        public static T BaseGet<T>(string settingNamespace, string settingName) => Digimezzo.Foundation.Core.Settings.SettingsClient.BaseGet<T>(settingNamespace, settingName);
        public static T Get<T>(string settingNamespace, string settingName) => Digimezzo.Foundation.Core.Settings.SettingsClient.Get<T>(settingNamespace, settingName);
        public static bool IsMigrationNeeded() => Digimezzo.Foundation.Core.Settings.SettingsClient.IsMigrationNeeded();
        public static bool IsSettingChanged(Digimezzo.Foundation.Core.Settings.SettingChangedEventArgs e, string @namespace, string name) => Digimezzo.Foundation.Core.Settings.SettingsClient.IsSettingChanged(e, @namespace, name);
        public static void Migrate() => Digimezzo.Foundation.Core.Settings.SettingsClient.Migrate();
        public static void Set<T>(string @namespace, string name, T value, bool raiseEvent = false) => Digimezzo.Foundation.Core.Settings.SettingsClient.Set(@namespace, name, value, raiseEvent);
        public static void Write() => Digimezzo.Foundation.Core.Settings.SettingsClient.Write();
    }
}
