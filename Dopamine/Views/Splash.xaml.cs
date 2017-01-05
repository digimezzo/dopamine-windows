using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;
using System;
using System.Threading.Tasks;
using System.Windows;
using Digimezzo.Utilities.Packaging;
using Digimezzo.Utilities.IO;

namespace Dopamine.Views
{
    public partial class Splash : Window
    {
        #region Variables
        private int uiWaitMilliSeconds = 300;
        private string errorMessage;
        private Package package;
        #endregion

        #region Properties
        public Package Package
        {
            get
            {
                return this.package;
            }
        }

        public bool IsPreview
        {
            get
            {
#if DEBUG
                return true;
#else
		        return false;
#endif
            }
        }

        public bool ShowErrorPanel
        {
            get { return Convert.ToBoolean(GetValue(ShowErrorPanelProperty)); }

            set { SetValue(ShowErrorPanelProperty, value); }
        }

        public bool ShowProgressRing
        {
            get { return Convert.ToBoolean(GetValue(ShowProgressRingProperty)); }

            set { SetValue(ShowProgressRingProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty ShowErrorPanelProperty = DependencyProperty.Register("ShowErrorPanel", typeof(bool), typeof(Splash), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowProgressRingProperty = DependencyProperty.Register("ShowProgressRing", typeof(bool), typeof(Splash), new PropertyMetadata(null));
        #endregion

        #region Construction
        public Splash()
        {
            Configuration config;
#if DEBUG
            config = Configuration.Debug;
#else
		    config = Configuration.Release;
#endif

            this.package = new Package(ProcessExecutable.Name(), ProcessExecutable.AssemblyVersion(), config);

            InitializeComponent();
        }
        #endregion

        #region private
        private void ShowError(string message)
        {
            this.ErrorMessage.Text = message;
            this.ShowErrorPanel = true;
        }

        private void ShowErrorDetails()
        {
            DateTime currentTime = DateTime.Now;
            string currentTimeString = currentTime.Year.ToString() + currentTime.Month.ToString() + currentTime.Day.ToString() + currentTime.Hour.ToString() + currentTime.Minute.ToString() + currentTime.Second.ToString() + currentTime.Millisecond.ToString();

            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ProductInformation.ApplicationDisplayName + "_" + currentTimeString + ".txt");
            System.IO.File.WriteAllText(path, this.errorMessage);
            System.Diagnostics.Process.Start(path);
        }

        private async void Initialize()
        {
            bool continueInitializing = true;

            // Give the UI some time to show the progress ring
            await Task.Delay(this.uiWaitMilliSeconds);

            if (continueInitializing)
            {
                // Check if Windows Media Foundation is installed
                continueInitializing = await this.CheckWindowsMediaFoundationAsync();
            }

            if (continueInitializing)
            {
                // Initialize the settings
                continueInitializing = await this.InitializeSettingsAsync();
            }

            if (continueInitializing)
            {
                // Initialize the database
                continueInitializing = await this.InitializeDatabaseAsync();
            }

            if (continueInitializing)
            {
                // If initializing was successful, start the application.
                if (this.ShowProgressRing)
                {
                    this.ShowProgressRing = false;

                    // Give the UI some time to hide the progress ring
                    await Task.Delay(this.uiWaitMilliSeconds);
                }

                Bootstrapper bootstrapper = new Bootstrapper();
                bootstrapper.Run();
                this.Close();
            }
            else
            {
                this.ShowError("I was not able to start. Please click 'Show details' for more information.");
            }
        }

        private async Task<bool> CheckWindowsMediaFoundationAsync()
        {
            bool isSuccess = false;

            try
            {
                await Task.Run(() =>
                {
                    isSuccess = System.IO.File.Exists(System.IO.Path.Combine(Environment.SystemDirectory, "mf.dll"));
                });
            }
            catch (Exception ex)
            {
                LogClient.Error("Windows Media Foundation could not be found. Exception: {0}", ex.Message);
                isSuccess = false;
            }

            if (!isSuccess)
            {
                this.errorMessage = "Windows Media Foundation was not found on your computer. It is required by " + ProductInformation.ApplicationDisplayName + " to play audio." + Environment.NewLine +
                                    "If you are using a 'N' version of Windows, please install the Media Feature Pack for your version of Windows." + Environment.NewLine + Environment.NewLine +
                                    "Media Feature Pack for Windows 7 N: https://www.microsoft.com/en-us/download/details.aspx?id=16546" + Environment.NewLine +
                                    "Media Feature Pack for Windows 8 N: https://www.microsoft.com/en-us/download/details.aspx?id=30685" + Environment.NewLine +
                                    "Media Feature Pack for Windows 10 N: https://www.microsoft.com/en-us/download/details.aspx?id=48231";
            }

            return isSuccess;
        }

        private async Task<bool> InitializeSettingsAsync()
        {

            bool isSuccess = false;

            try
            {
                // Checks if an upgrade of the settings is needed
                if (SettingsClient.IsUpgradeNeeded())
                {
                    this.ShowProgressRing = true;
                    LogClient.Info("Upgrading settings");
                    await Task.Run(() => SettingsClient.Upgrade());
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem initializing the settings. Exception: {0}", ex.Message);
                this.errorMessage = ex.Message;
                isSuccess = false;
            }

            return isSuccess;
        }

        private async Task<bool> InitializeDatabaseAsync()
        {
            bool isSuccess = false;

            try
            {
                var migrator = new DbMigrator();

                if (!migrator.DatabaseExists())
                {
                    // Create the database if it doesn't exist
                    this.ShowProgressRing = true;
                    LogClient.Info("Creating database");
                    await Task.Run(() => migrator.InitializeNewDatabase());
                }
                else
                {
                    // Upgrade the database if it is not the latest version

                    if (migrator.DatabaseNeedsUpgrade())
                    {
                        this.ShowProgressRing = true;
                        LogClient.Info("Upgrading database");
                        await Task.Run(() => migrator.UpgradeDatabase());
                    }
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem initializing the database. Exception: {0}", ex.Message);
                this.errorMessage = ex.Message;
                isSuccess = false;
            }

            return isSuccess;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void BtnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void BtnShowDetails_Click(object sender, RoutedEventArgs e)
        {
            this.ShowErrorDetails();
        }
        #endregion
    }
}
