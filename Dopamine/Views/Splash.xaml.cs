using Dopamine.Core.Database;
using Dopamine.Core.Logging;
using Dopamine.Core.Settings;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Windows;

namespace Dopamine.Views
{
    public partial class Splash : Window
    {
        #region Variables
        private int uiWaitMilliSeconds = 300;
        private string errorMessage;
        #endregion

        #region Properties
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

            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Dopamine_" + currentTimeString + ".txt");
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

        private async Task<bool> InitializeSettingsAsync()
        {

            bool isInitializeSettingsSuccess = false;

            try
            {
                // Checks if an upgrade of the settings is needed
                if (XmlSettingsClient.Instance.IsSettingsUpgradeNeeded())
                {
                    this.ShowProgressRing = true;
                    LogClient.Instance.Logger.Info("Upgrading settings");
                    await Task.Run(() => XmlSettingsClient.Instance.UpgradeSettings());
                }

                isInitializeSettingsSuccess = true;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem initializing the settings. Exception: {0}", ex.Message);
                this.errorMessage = ex.Message;
                isInitializeSettingsSuccess = false;
            }

            return isInitializeSettingsSuccess;
        }

        private async Task<bool> InitializeDatabaseAsync()
        {

            bool isInitializeDatabaseSuccess = false;

            try
            {
                bool needsMetadataInit = false;

                var con = new SQLiteConnection(DbConnection.ConnectionString);
                var dbm = new DbCreator(con);

                if (!DbCreator.DatabaseExists())
                {
                    // Create the database if it doesn't exist
                    this.ShowProgressRing = true;
                    LogClient.Instance.Logger.Info("Creating database");
                    await Task.Run(() => dbm.InitializeNewDatabase());

                    needsMetadataInit = true;
                }
                else
                {
                    // Upgrade the database if it is not the latest version

                    if (dbm.DatabaseNeedsUpgrade())
                    {
                        this.ShowProgressRing = true;
                        LogClient.Instance.Logger.Info("Upgrading database");
                        await Task.Run(() => dbm.UpgradeDatabase());

                        needsMetadataInit = true;
                    }
                }

                if (needsMetadataInit)
                {
                    this.ShowProgressRing = true;
                    LogClient.Instance.Logger.Info("Initializing EntityFramework MetaData");
                    await Utils.InitializeEntityFrameworkMetaDataAsync();
                }

                isInitializeDatabaseSuccess = true;
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem initializing the database. Exception: {0}", ex.Message);
                this.errorMessage = ex.Message;
                isInitializeDatabaseSuccess = false;
            }

            return isInitializeDatabaseSuccess;
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
