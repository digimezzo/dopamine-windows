using Dopamine.Core.Base;
using Dopamine.Core.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Linq;

namespace Dopamine.Core.Settings
{
    public class XmlSettingsClient : ISettingsClient
    {
        #region Variables
        private static XmlSettingsClient instance;
        private Timer timer;
        private object timerMutex = new object();
        private bool delayWrite;
        private string baseSettingsFile = System.IO.Path.Combine(ApplicationPaths.ExecutionFolder, "BaseSettings.xml");
        private XDocument baseSettingsDoc;
        private string applicationFolder;
        private string settingsFile;
        private XDocument settingsDoc;
        #endregion

        #region Properties
        public string SettingsFile
        {
            get { return this.settingsFile; }
        }
        public string ApplicationFolder
        {
            get { return this.applicationFolder; }
        }
        #endregion

        #region Construction
        private XmlSettingsClient()
        {
            this.timer = new System.Timers.Timer(100); // a 10th of a second
            this.timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);

            // Check in BaseSettings.xml if we're using the portable application
            this.baseSettingsDoc = XDocument.Load(this.baseSettingsFile);

            if (this.applicationFolder == null)
            {
                bool isPortable = false;

                // Set the path of Settings.xml
                if (this.BaseTryGet<bool>("Application", "IsPortable", ref isPortable))
                {
                    // Sets the application folder
                    if (isPortable)
                    {
                        this.applicationFolder = System.IO.Path.Combine(ApplicationPaths.ExecutionFolder, ProductInformation.ApplicationAssemblyName);
                    }
                    else
                    {
                        this.applicationFolder = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName);
                    }
                }
                else
                {
                    // By default, we save in the user's Roaming folder
                    this.applicationFolder = System.IO.Path.Combine(LegacyPaths.AppData(), ProductInformation.ApplicationAssemblyName);
                }

                this.TryCreateApplicationFolder();
            }

            this.settingsFile = System.IO.Path.Combine(ApplicationFolder, "Settings.xml");

            // Check if Settings.xml exists in the given path. If not,
            // create a new Settings.xml based on BaseSettings.xml
            if (!File.Exists(this.settingsFile))
            {
                File.Copy(this.baseSettingsFile, this.settingsFile);
            }

            try
            {
                // Load Settings.xml in memory
                this.settingsDoc = XDocument.Load(this.settingsFile);
            }
            catch (Exception)
            {
                // After a crash, the Settings file is sometimes empty.  If that
                // happens, copy the BaseSettings file (there is no way to restore
                // settings from a broken file anyway) and try to load the Settings
                // file again. 
                File.Copy(this.baseSettingsFile, this.settingsFile, true);
                this.settingsDoc = XDocument.Load(this.settingsFile);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this.timerMutex)
            {
                if (this.delayWrite)
                {
                    this.delayWrite = false;
                }
                else
                {
                    this.timer.Stop();
                    this.settingsDoc.Save(this.settingsFile);
                }
            }
        }

        public static XmlSettingsClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XmlSettingsClient();
                }
                return instance;
            }
        }
        #endregion

        #region Private
        private bool SettingExists<T>(string settingNamespace, string settingName)
        {
            T value = this.Get<T>(settingNamespace, settingName);

            return value != null;
        }

        private void TryCreateApplicationFolder()
        {
            if (!Directory.Exists(this.ApplicationFolder))
            {
                Directory.CreateDirectory(this.ApplicationFolder);
            }
        }

        // Queue XML file writes to minimize disk access
        private void QueueWrite()
        {
            lock (this.timerMutex)
            {
                if (!this.timer.Enabled)
                {
                    this.timer.Start();
                }
                else
                {
                    this.delayWrite = true;
                }
            }
        }
        #endregion

        #region ISettingsClient
        // Provides immediate writing of settings to the XML file
        public void Write()
        {
            this.timer.Stop();
            this.delayWrite = false;
            this.settingsDoc.Save(this.settingsFile);
        }

        public bool IsSettingsUpgradeNeeded()
        {
            bool returnValue = false;

            // Check if the existing Settings.xml is out of date
            if (this.Get<int>("Settings", "Version") < this.BaseGet<int>("Settings", "Version"))
            {
                returnValue = true;
            }

            return returnValue;
        }

        public void UpgradeSettings()
        {
            // Get the old settings
            List<SettingEntry> oldSettings = default(List<SettingEntry>);

            oldSettings = (from n in this.settingsDoc.Element("Settings").Elements("Namespace")
                           from s in n.Elements("Setting")
                           from v in s.Elements("Value")
                           where !n.Attribute("Name").Value.ToString().ToLower().Equals("settings")
                           select new SettingEntry
                           {
                               Namespace = n.Attribute("Name").Value,
                               Setting = s.Attribute("Name").Value,
                               Value = v.Value
                           }).ToList();

            // Create a new Settings file, based on the new BaseSettings file
            File.Copy(this.baseSettingsFile, this.settingsFile, true);

            // Load the new Settings file in memory
            this.settingsDoc = XDocument.Load(this.settingsFile);

            // Try to write the old settings in the new Settings file
            foreach (SettingEntry item in oldSettings)
            {
                try
                {
                    if (SettingExists<string>(item.Namespace, item.Setting))
                    {
                        // We don't know the type of the setting. So set all settings as String
                        Set<string>(item.Namespace, item.Setting, item.Value);
                    }
                }
                catch (Exception)
                {
                    // If we fail, we do nothing.
                }
            }
        }

        public void Set<T>(string settingNamespace, string settingName, T value)
        {
            lock (this.settingsDoc)
            {
                XElement setting = (from n in this.settingsDoc.Element("Settings").Elements("Namespace")
                                    from s in n.Elements("Setting")
                                    from v in s.Elements("Value")
                                    where n.Attribute("Name").Value.Equals(settingNamespace) && s.Attribute("Name").Value.Equals(settingName)
                                    select v).FirstOrDefault();

                if (setting != null)
                {
                    setting.SetValue(value);
                }

                this.QueueWrite();
            }
        }

        public bool TryGet<T>(string settingNamespace, string settingName, ref T value)
        {
            value = this.Get<T>(settingNamespace, settingName);

            return value != null;
        }

        public T Get<T>(string settingNamespace, string settingName)
        {
            lock (this.settingsDoc)
            {
                XElement setting = (from n in this.settingsDoc.Element("Settings").Elements("Namespace")
                                    from s in n.Elements("Setting")
                                    from v in s.Elements("Value")
                                    where n.Attribute("Name").Value.Equals(settingNamespace) && s.Attribute("Name").Value.Equals(settingName)
                                    select v).FirstOrDefault();

                // For numbers, we need to provide CultureInfo.InvariantCulture. 
                // Otherwise, deserializing from XML can cause a FormatException.
                if (typeof(T) == typeof(float))
                {
                    float floatValue;
                    float.TryParse(setting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out floatValue);
                    return (T)Convert.ChangeType(floatValue, typeof(T));
                }
                else if (typeof(T) == typeof(double))
                {
                    float doubleValue;
                    float.TryParse(setting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out doubleValue);
                    return (T)Convert.ChangeType(doubleValue, typeof(T));
                }
                else
                {
                    return (T)Convert.ChangeType(setting.Value, typeof(T));
                }
            }
        }

        public T BaseGet<T>(string settingNamespace, string settingName)
        {

            lock (this.baseSettingsDoc)
            {
                XElement baseSetting = (from n in this.baseSettingsDoc.Element("Settings").Elements("Namespace")
                                        from s in n.Elements("Setting")
                                        from v in s.Elements("Value")
                                        where n.Attribute("Name").Value.Equals(settingNamespace) && s.Attribute("Name").Value.Equals(settingName)
                                        select v).FirstOrDefault();

                // For numbers, we need to provide CultureInfo.InvariantCulture. 
                // Otherwise, deserializing from XML can cause a FormatException.
                if (typeof(T) == typeof(float))
                {
                    float floatValue;
                    float.TryParse(baseSetting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out floatValue);
                    return (T)Convert.ChangeType(floatValue, typeof(T));
                }
                else if (typeof(T) == typeof(double))
                {
                    float doubleValue;
                    float.TryParse(baseSetting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out doubleValue);
                    return (T)Convert.ChangeType(doubleValue, typeof(T));
                }
                else
                {
                    return (T)Convert.ChangeType(baseSetting.Value, typeof(T));
                }
            }
        }

        public bool BaseTryGet<T>(string settingNamespace, string settingName, ref T value)
        {
            value = this.BaseGet<T>(settingNamespace, settingName);

            return value != null;
        }
        #endregion

        #region Event Handlers
        private void OnTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            lock (this.timerMutex)
            {
                if (this.delayWrite)
                {
                    this.delayWrite = false;
                }
                else
                {
                    this.timer.Stop();
                    this.settingsDoc.Save(this.settingsFile);
                }
            }
        }
        #endregion
    }
}
