using Dopamine.Core.Base;
using Dopamine.Core.Settings;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Dopamine.Core.Database
{
    public class DbConnection
    {
        #region Shared Properties
        private static string databaseFile;
        private static string connectionString;
        #endregion

        #region Properties
        public static string DatabaseFile
        {
            get
            {
                if (databaseFile == null)
                {
                    databaseFile = Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ProductInformation.ApplicationAssemblyName + ".db");
                }
                return databaseFile;
            }
        }

        public static string ConnectionString
        {
            get { return "Data Source=" + DatabaseFile + ";Version=3;Pooling=True;Max Pool Size=100"; }
        }
        #endregion

        #region Public
        public static void ExecuteNonQuery(string query)
        {
            SQLiteConnection con = new SQLiteConnection();
            SQLiteCommand cmd = new SQLiteCommand();

            con.ConnectionString = DbConnection.ConnectionString;
            con.Open();

            cmd = con.CreateCommand();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            con.Close();
        }

        public static T ExecuteQuery<T>(string query)
        {
            SQLiteConnection con = new SQLiteConnection();
            SQLiteCommand cmd = new SQLiteCommand();

            con.ConnectionString = DbConnection.ConnectionString;
            con.Open();

            cmd = con.CreateCommand();
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;

            object obj = cmd.ExecuteScalar();

            cmd.Dispose();
            con.Close();

            return (T)obj;
        }
        #endregion
    }
}
