using Dopamine.Core.Base;
using Dopamine.Core.Logging;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace Dopamine.Core.Database
{
    public class DbCreator
    {
        #region DatabaseVersionAttribute
        protected sealed class DatabaseVersionAttribute : Attribute
        {
            private int version;

            public DatabaseVersionAttribute(int version)
            {
                this.version = version;
            }

            public int Version
            {
                get { return this.version; }
            }
        }
        #endregion

        #region Variables
        // NOTE: whenever there is a change in the database schema,
        // this version MUST be incremented and a migration method
        // MUST be supplied to match the new version number
        protected const int CURRENT_VERSION = 10;
        private SQLiteConnection connection;
        private int userDatabaseVersion;
        #endregion

        #region Construction
        public DbCreator(SQLiteConnection connection)
        {
            this.connection = connection;
        }
        #endregion

        #region Fresh database setup
        private void CreateConfiguration()
        {
            this.Execute("CREATE TABLE Configurations (" +
                         "ConfigurationID    INTEGER," +
                         "Key                TEXT," +
                         "Value              TEXT," +
                         "PRIMARY KEY(ConfigurationID));");

            this.Execute(String.Format("INSERT INTO Configurations (ConfigurationID, Key, Value) VALUES (null,'DatabaseVersion', {0})", CURRENT_VERSION));
        }

        private void CreateTablesAndIndexes()
        {
            this.Execute("CREATE TABLE Artists (" +
                         "ArtistID           INTEGER," +
                         "ArtistName	     TEXT," +
                         "PRIMARY KEY(ArtistID));");

            this.Execute("CREATE INDEX ArtistsIndex ON Artists(ArtistName)");

            this.Execute("CREATE TABLE Genres (" +
                         "GenreID           INTEGER," +
                         "GenreName	        TEXT," +
                         "PRIMARY KEY(GenreID));");

            this.Execute("CREATE INDEX GenresIndex ON Genres(GenreName)");

            this.Execute("CREATE TABLE Albums (" +
                         "AlbumID	        INTEGER," +
                         "AlbumTitle	    TEXT," +
                         "AlbumArtist	    TEXT," +
                         "Year	            INTEGER," +
                         "ArtworkID	        TEXT," +
                         "DateLastSynced	INTEGER," +
                         "DateAdded	        INTEGER," +
                         "PRIMARY KEY(AlbumID));");

            this.Execute("CREATE INDEX AlbumsIndex ON Albums(AlbumTitle, AlbumArtist)");
            this.Execute("CREATE INDEX AlbumsYearIndex ON Albums(Year);");

            this.Execute("CREATE TABLE Playlists (" +
                         "PlaylistID	    INTEGER," +
                         "PlaylistName	    TEXT," +
                         "PRIMARY KEY(PlaylistID));");

            this.Execute("CREATE TABLE PlaylistEntries (" +
                         "EntryID	        INTEGER," +
                         "PlaylistID	    INTEGER," +
                         "TrackID	        INTEGER," +
                         "PRIMARY KEY(EntryID));");

            this.Execute("CREATE TABLE Folders (" +
                         "FolderID	         INTEGER PRIMARY KEY AUTOINCREMENT," +
                         "Path	             TEXT," +
                         "ShowInCollection   INTEGER);");

            this.Execute("CREATE TABLE Tracks (" +
                         "TrackID	            INTEGER," +
                         "ArtistID	            INTEGER," +
                         "GenreID	            INTEGER," +
                         "AlbumID	            INTEGER," +
                         "FolderID	            INTEGER," +
                         "Path	                TEXT," +
                         "FileName	            TEXT," +
                         "MimeType	            TEXT," +
                         "FileSize	            INTEGER," +
                         "BitRate	            INTEGER," +
                         "SampleRate	        INTEGER," +
                         "TrackTitle	        TEXT," +
                         "TrackNumber	        INTEGER," +
                         "TrackCount	        INTEGER," +
                         "DiscNumber	        INTEGER," +
                         "DiscCount	            INTEGER," +
                         "Duration	            INTEGER," +
                         "Year	                INTEGER," +
                         "Rating	            INTEGER," +
                         "PlayCount	            INTEGER," +
                         "SkipCount	            INTEGER," +
                         "DateAdded  	        INTEGER," +
                         "DateLastPlayed        INTEGER," +
                         "DateLastSynced	    INTEGER," +
                         "DateFileModified	    INTEGER," +
                         "MetaDataHash	        TEXT," +
                         "PRIMARY KEY(TrackID));");

            this.Execute("CREATE INDEX TracksArtistIDIndex ON Tracks(ArtistID);");
            this.Execute("CREATE INDEX TracksAlbumIDIndex ON Tracks(AlbumID);");
            this.Execute("CREATE INDEX TracksGenreIDIndex ON Tracks(GenreID);");
            this.Execute("CREATE INDEX TracksFolderIDIndex ON Tracks(FolderID);");
            this.Execute("CREATE INDEX TracksPathIndex ON Tracks(Path)");

            this.Execute("CREATE TABLE RemovedTracks (" +
                         "TrackID	            INTEGER," +
                         "Path	                TEXT," +
                         "DateRemoved           INTEGER," +
                         "PRIMARY KEY(TrackID));");

            this.Execute("CREATE TABLE QueuedTracks (" +
                         "QueuedTrackID         INTEGER," +
                         "Path	                TEXT," +
                         "OrderID               INTEGER," +
                         "PRIMARY KEY(QueuedTrackID));");

            this.Execute("CREATE TABLE IndexingStatistics (" +
                        "IndexingStatisticID    INTEGER," +
                        "Key                    TEXT," +
                        "Value                  TEXT," +
                        "PRIMARY KEY(IndexingStatisticID));");
        }
        #endregion

        #region Version 1
        [DatabaseVersion(1)]
        private void Migrate1()
        {
            this.Execute("ALTER TABLE Tracks ADD DiscNumber INTEGER;");
            this.Execute("ALTER TABLE Tracks ADD DiscCount INTEGER;");

            this.Execute("UPDATE Tracks SET DiscNumber=(SELECT DiscNumber FROM Albums WHERE Albums.AlbumID=Tracks.AlbumID);");
            this.Execute("UPDATE Tracks SET DiscCount=(SELECT DiscCount FROM Albums WHERE Albums.AlbumID=Tracks.AlbumID);");

            this.Execute("CREATE TABLE Albums_Backup (" +
                         "AlbumID	            INTEGER," +
                         "AlbumTitle	        TEXT," +
                         "AlbumArtist	        TEXT," +
                         "EmbeddedArtworkID	    TEXT," +
                         "EmbeddedArtworkSize   INTEGER," +
                         "ExternalArtworkID	    TEXT," +
                         "ExternalArtworkSize   INTEGER," +
                         "ExternalArtworkPath	TEXT," +
                         "ExternalArtworkDateFileModified	INTEGER," +
                         "First_AlbumID INTEGER," +
                         "PRIMARY KEY(AlbumID));");

            this.Execute("INSERT INTO Albums_Backup SELECT AlbumID," +
                         "AlbumTitle," +
                         "AlbumArtist," +
                         "EmbeddedArtworkID," +
                         "EmbeddedArtworkSize," +
                         "ExternalArtworkID," +
                         "ExternalArtworkSize," +
                         "ExternalArtworkPath," +
                         "ExternalArtworkDateFileModified, (SELECT AlbumID FROM Albums ab WHERE LOWER(TRIM(a.AlbumTitle))=LOWER(TRIM(ab.AlbumTitle)) AND LOWER(TRIM(a.AlbumArtist))=LOWER(TRIM(ab.AlbumArtist)) ORDER BY AlbumID LIMIT 1) " +
                         "FROM Albums a;");

            this.Execute("UPDATE Tracks SET AlbumID=(SELECT First_AlbumID FROM Albums_Backup WHERE Albums_Backup.AlbumID=Tracks.AlbumID);");
            this.Execute("DROP TABLE Albums;");

            this.Execute("CREATE TABLE Albums (" +
                         "AlbumID	            INTEGER," +
                         "AlbumTitle	        TEXT," +
                         "AlbumArtist	        TEXT," +
                         "EmbeddedArtworkID	    TEXT," +
                         "EmbeddedArtworkSize   INTEGER," +
                         "ExternalArtworkID	    TEXT," +
                         "ExternalArtworkSize   INTEGER," +
                         "ExternalArtworkPath	TEXT," +
                          "ExternalArtworkDateFileModified	INTEGER," +
                        "PRIMARY KEY(AlbumID));");

            this.Execute("INSERT INTO Albums SELECT AlbumID," +
                         "AlbumTitle," +
                         "AlbumArtist," +
                         "EmbeddedArtworkID," +
                         "EmbeddedArtworkSize," +
                         "ExternalArtworkID," +
                         "ExternalArtworkSize," +
                         "ExternalArtworkPath," +
                         "ExternalArtworkDateFileModified " +
                         "FROM Albums_Backup WHERE AlbumID=First_AlbumID;");

            this.Execute("DROP TABLE Albums_Backup;");

            this.Execute("VACUUM;");
        }
        #endregion

        #region Version 2
        [DatabaseVersion(2)]
        private void Migrate2()
        {
            this.Execute("CREATE TABLE Albums_Backup (" +
                         "AlbumID	        INTEGER," +
                         "AlbumTitle	    TEXT," +
                         "AlbumArtist	    TEXT," +
                         "PRIMARY KEY(AlbumID));");

            this.Execute("INSERT INTO Albums_Backup SELECT AlbumID," +
                         "AlbumTitle," +
                         "AlbumArtist " +
                         "FROM Albums;");

            this.Execute("DROP TABLE Albums;");

            this.Execute("CREATE TABLE Albums (" +
                         "AlbumID	        INTEGER," +
                         "AlbumTitle	    TEXT," +
                         "AlbumArtist	    TEXT," +
                         "Year	            INTEGER," +
                         "ArtworkID	        TEXT," +
                         "DateLastSynced	INTEGER," +
                         "PRIMARY KEY(AlbumID));");

            this.Execute("INSERT INTO Albums SELECT AlbumID," +
                         "AlbumTitle," +
                         "AlbumArtist," +
                         "0," +
                         "null," +
                         "0 " +
                         "FROM Albums_Backup;");

            this.Execute("DROP TABLE Albums_Backup;");

            this.Execute("CREATE INDEX IF NOT EXISTS TracksFolderIDIndex ON Tracks(FolderID);");
            this.Execute("CREATE INDEX IF NOT EXISTS TracksArtistIDIndex ON Tracks(ArtistID);");
            this.Execute("CREATE INDEX IF NOT EXISTS TracksAlbumIDIndex ON Tracks(AlbumID);");
            this.Execute("CREATE INDEX IF NOT EXISTS TracksPathIndex ON Tracks(Path);");
            this.Execute("CREATE INDEX IF NOT EXISTS ArtistsIndex ON Artists(ArtistName);");
            this.Execute("CREATE INDEX IF NOT EXISTS AlbumsIndex ON Albums(AlbumTitle, AlbumArtist);");

            this.Execute("VACUUM;");
        }
        #endregion

        #region Version 3
        [DatabaseVersion(3)]
        private void Migrate3()
        {
            this.Execute("CREATE TABLE RemovedTracks (" +
                        "TrackID	            INTEGER," +
                        "Path	                TEXT," +
                        "DateRemoved            INTEGER," +
                        "PRIMARY KEY(TrackID));");

            this.Execute("BEGIN TRANSACTION;");
            this.Execute("CREATE TEMPORARY TABLE Tracks_Backup (" +
                                 "TrackID	            INTEGER," +
                                 "ArtistID	            INTEGER," +
                                 "AlbumID	            INTEGER," +
                                 "Path	                TEXT," +
                                 "FileName	            TEXT," +
                                 "MimeType	            TEXT," +
                                 "FileSize	            INTEGER," +
                                 "BitRate	            INTEGER," +
                                 "SampleRate	        INTEGER," +
                                 "TrackTitle	        TEXT," +
                                 "TrackNumber	        INTEGER," +
                                 "TrackCount	        INTEGER," +
                                 "DiscNumber	        INTEGER," +
                                 "DiscCount	            INTEGER," +
                                 "Duration	            INTEGER," +
                                 "Year	                INTEGER," +
                                 "Genre	                TEXT," +
                                 "Rating	            INTEGER," +
                                 "PlayCount	            INTEGER," +
                                 "SkipCount	            INTEGER," +
                                 "DateAdded  	        INTEGER," +
                                 "DateLastPlayed        INTEGER," +
                                 "DateLastSynced	    INTEGER," +
                                 "DateFileModified	    INTEGER," +
                                 "MetaDataHash	        TEXT," +
                                 "PRIMARY KEY(TrackID));");

            this.Execute("INSERT INTO Tracks_Backup SELECT TrackID," +
                                 "ArtistID," +
                                 "AlbumID," +
                                 "Path," +
                                 "FileName," +
                                 "MimeType," +
                                 "FileSize," +
                                 "BitRate," +
                                 "SampleRate," +
                                 "TrackTitle," +
                                 "TrackNumber," +
                                 "TrackCount," +
                                 "DiscNumber," +
                                 "DiscCount," +
                                 "Duration," +
                                 "Year," +
                                 "Genre," +
                                 "Rating," +
                                 "PlayCount," +
                                 "SkipCount," +
                                 "DateAdded," +
                                 "DateLastPlayed," +
                                 "DateLastSynced," +
                                 "DateFileModified," +
                                 "MetaDataHash " +
                                 "FROM Tracks;");

            this.Execute("DROP TABLE Tracks;");

            this.Execute("CREATE TABLE Tracks (" +
                         "TrackID	            INTEGER," +
                         "ArtistID	            INTEGER," +
                         "AlbumID	            INTEGER," +
                         "Path	                TEXT," +
                         "FileName	            TEXT," +
                         "MimeType	            TEXT," +
                         "FileSize	            INTEGER," +
                         "BitRate	            INTEGER," +
                         "SampleRate	        INTEGER," +
                         "TrackTitle	        TEXT," +
                         "TrackNumber	        INTEGER," +
                         "TrackCount	        INTEGER," +
                         "DiscNumber	        INTEGER," +
                         "DiscCount	            INTEGER," +
                         "Duration	            INTEGER," +
                         "Year	                INTEGER," +
                         "Genre	                TEXT," +
                         "Rating	            INTEGER," +
                         "PlayCount	            INTEGER," +
                         "SkipCount	            INTEGER," +
                         "DateAdded  	        INTEGER," +
                         "DateLastPlayed        INTEGER," +
                         "DateLastSynced	    INTEGER," +
                         "DateFileModified	    INTEGER," +
                         "MetaDataHash	        TEXT," +
                         "PRIMARY KEY(TrackID));");

            this.Execute("INSERT INTO Tracks SELECT TrackID," +
                                "ArtistID," +
                                "AlbumID," +
                                "Path," +
                                "FileName," +
                                "MimeType," +
                                "FileSize," +
                                "BitRate," +
                                "SampleRate," +
                                "TrackTitle," +
                                "TrackNumber," +
                                "TrackCount," +
                                "DiscNumber," +
                                "DiscCount," +
                                "Duration," +
                                "Year," +
                                "Genre," +
                                "Rating," +
                                "PlayCount," +
                                "SkipCount," +
                                "DateAdded," +
                                "DateLastPlayed," +
                                "DateLastSynced," +
                                "DateFileModified," +
                                "MetaDataHash " +
                                "FROM Tracks_Backup;");

            this.Execute("DROP TABLE Tracks_Backup;");

            this.Execute("COMMIT;");

            this.Execute("CREATE INDEX IF NOT EXISTS TracksArtistIDIndex ON Tracks(ArtistID);");
            this.Execute("CREATE INDEX IF NOT EXISTS TracksAlbumIDIndex ON Tracks(AlbumID);");
            this.Execute("CREATE INDEX TracksPathIndex ON Tracks(Path)");

            this.Execute("ALTER TABLE Albums ADD DateAdded INTEGER;");
            this.Execute("UPDATE Albums SET DateAdded=(SELECT MIN(DateAdded) FROM Tracks WHERE Tracks.AlbumID = Albums.AlbumID);");

            this.Execute("VACUUM;");
        }
        #endregion

        #region Version 4
        [DatabaseVersion(4)]
        private void Migrate4()
        {
            this.Execute("CREATE TABLE Genres (" +
                         "GenreID           INTEGER," +
                         "GenreName	        TEXT," +
                         "PRIMARY KEY(GenreID));");

            this.Execute("ALTER TABLE Tracks ADD GenreID INTEGER;");

            this.Execute("INSERT INTO Genres(GenreName) SELECT DISTINCT Genre FROM Tracks WHERE TRIM(Genre) <>'';");
            this.Execute("UPDATE Tracks SET GenreID=(SELECT GenreID FROM Genres WHERE Genres.GenreName=Tracks.Genre) WHERE TRIM(Genre) <> '';");

            this.Execute(String.Format("INSERT INTO Genres(GenreName) VALUES('{0}');", Defaults.UnknownGenreString));
            this.Execute(String.Format("UPDATE Tracks SET GenreID=(SELECT GenreID FROM Genres WHERE Genres.GenreName='{0}') WHERE TRIM(Genre) = '';", Defaults.UnknownGenreString));

            this.Execute("CREATE TABLE Tracks_Backup (" +
                         "TrackID	            INTEGER," +
                         "ArtistID	            INTEGER," +
                         "GenreID	            INTEGER," +
                         "AlbumID	            INTEGER," +
                         "Path	                TEXT," +
                         "FileName	            TEXT," +
                         "MimeType	            TEXT," +
                         "FileSize	            INTEGER," +
                         "BitRate	            INTEGER," +
                         "SampleRate	        INTEGER," +
                         "TrackTitle	        TEXT," +
                         "TrackNumber	        INTEGER," +
                         "TrackCount	        INTEGER," +
                         "DiscNumber	        INTEGER," +
                         "DiscCount	            INTEGER," +
                         "Duration	            INTEGER," +
                         "Year	                INTEGER," +
                         "Rating	            INTEGER," +
                         "PlayCount	            INTEGER," +
                         "SkipCount	            INTEGER," +
                         "DateAdded  	        INTEGER," +
                         "DateLastPlayed        INTEGER," +
                         "DateLastSynced	    INTEGER," +
                         "DateFileModified	    INTEGER," +
                         "MetaDataHash	        TEXT," +
                         "PRIMARY KEY(TrackID));");

            this.Execute("INSERT INTO Tracks_Backup SELECT TrackID," +
                                 "ArtistID," +
                                 "GenreID," +
                                 "AlbumID," +
                                 "Path," +
                                 "FileName," +
                                 "MimeType," +
                                 "FileSize," +
                                 "BitRate," +
                                 "SampleRate," +
                                 "TrackTitle," +
                                 "TrackNumber," +
                                 "TrackCount," +
                                 "DiscNumber," +
                                 "DiscCount," +
                                 "Duration," +
                                 "Year," +
                                 "Rating," +
                                 "PlayCount," +
                                 "SkipCount," +
                                 "DateAdded," +
                                 "DateLastPlayed," +
                                 "DateLastSynced," +
                                 "DateFileModified," +
                                 "MetaDataHash " +
                                 "FROM Tracks;");

            this.Execute("DROP TABLE Tracks;");

            this.Execute("CREATE TABLE Tracks (" +
                         "TrackID	            INTEGER," +
                         "ArtistID	            INTEGER," +
                         "GenreID	            INTEGER," +
                         "AlbumID	            INTEGER," +
                         "Path	                TEXT," +
                         "FileName	            TEXT," +
                         "MimeType	            TEXT," +
                         "FileSize	            INTEGER," +
                         "BitRate	            INTEGER," +
                         "SampleRate	        INTEGER," +
                         "TrackTitle	        TEXT," +
                         "TrackNumber	        INTEGER," +
                         "TrackCount	        INTEGER," +
                         "DiscNumber	        INTEGER," +
                         "DiscCount	            INTEGER," +
                         "Duration	            INTEGER," +
                         "Year	                INTEGER," +
                         "Rating	            INTEGER," +
                         "PlayCount	            INTEGER," +
                         "SkipCount	            INTEGER," +
                         "DateAdded  	        INTEGER," +
                         "DateLastPlayed        INTEGER," +
                         "DateLastSynced	    INTEGER," +
                         "DateFileModified	    INTEGER," +
                         "MetaDataHash	        TEXT," +
                         "PRIMARY KEY(TrackID));");

            this.Execute("INSERT INTO Tracks SELECT TrackID," +
                               "ArtistID," +
                               "GenreID," +
                               "AlbumID," +
                               "Path," +
                               "FileName," +
                               "MimeType," +
                               "FileSize," +
                               "BitRate," +
                               "SampleRate," +
                               "TrackTitle," +
                               "TrackNumber," +
                               "TrackCount," +
                               "DiscNumber," +
                               "DiscCount," +
                               "Duration," +
                               "Year," +
                               "Rating," +
                               "PlayCount," +
                               "SkipCount," +
                               "DateAdded," +
                               "DateLastPlayed," +
                               "DateLastSynced," +
                               "DateFileModified," +
                               "MetaDataHash " +
                               "FROM Tracks_Backup;");

            this.Execute("DROP TABLE Tracks_Backup;");

            this.Execute("CREATE INDEX IF NOT EXISTS TracksGenreIDIndex ON Tracks(GenreID);");
            this.Execute("CREATE INDEX GenresIndex ON Genres(GenreName);");
        }
        #endregion

        #region Version 5
        [DatabaseVersion(5)]
        private void Migrate5()
        {
            this.Execute("UPDATE Albums SET Year=(SELECT MAX(Year) FROM Tracks WHERE Tracks.AlbumID=Albums.AlbumID) WHERE AlbumTitle<>'Unknown Album';");
            this.Execute("CREATE INDEX IF NOT EXISTS AlbumsYearIndex ON Albums(Year);");
        }
        #endregion

        #region Version 6
        [DatabaseVersion(6)]
        private void Migrate6()
        {
            this.Execute("ALTER TABLE Tracks ADD FolderID INTEGER;");
            this.Execute("UPDATE Tracks SET FolderID=(SELECT FolderID FROM Folders WHERE UPPER(Tracks.Path) LIKE UPPER(Folders.Path)||'%');");

            this.Execute("CREATE INDEX IF NOT EXISTS TracksFolderIDIndex ON Tracks(FolderID);");
        }
        #endregion

        #region Version 7
        [DatabaseVersion(7)]
        private void Migrate7()
        {
            this.Execute("ALTER TABLE Folders ADD ShowInCollection INTEGER;");
            this.Execute("UPDATE Folders SET ShowInCollection=1;");
        }
        #endregion

        #region Version 8
        [DatabaseVersion(8)]
        private void Migrate8()
        {
            this.Execute("CREATE TABLE QueuedTracks (" +
                          "QueuedTrackID     INTEGER," +
                          "Path	             TEXT," +
                          "OrderID           INTEGER," +
                          "PRIMARY KEY(QueuedTrackID));");
        }
        #endregion

        #region Version 9
        [DatabaseVersion(9)]
        private void Migrate9()
        {
            this.Execute("CREATE TABLE IndexingStatistics (" +
                         "IndexingStatisticID    INTEGER," +
                         "Key                    TEXT," +
                         "Value                  TEXT," +
                         "PRIMARY KEY(IndexingStatisticID));");
       }
        #endregion

        #region Version 10
        [DatabaseVersion(10)]
        private void Migrate10()
        {
            this.Execute("BEGIN TRANSACTION;");

            this.Execute("CREATE TABLE Folders_backup (" +
                         "FolderID	         INTEGER," +
                         "Path	             TEXT," +
                         "ShowInCollection   INTEGER," +
                         "PRIMARY KEY(FolderID));");

            this.Execute("INSERT INTO Folders_backup SELECT * FROM Folders;");

            this.Execute("DROP TABLE Folders;");

            this.Execute("CREATE TABLE Folders (" +
                         "FolderID	         INTEGER PRIMARY KEY AUTOINCREMENT," +
                         "Path	             TEXT," +
                         "ShowInCollection   INTEGER);");
           
            this.Execute("INSERT INTO Folders SELECT * FROM Folders_backup;");

            this.Execute("DROP TABLE Folders_backup;");

            this.Execute("COMMIT;");

            this.Execute("VACUUM;");
        }
        #endregion

        #region Public
        public static bool DatabaseExists()
        {
            return File.Exists(DbConnection.DatabaseFile);
        }

        public bool DatabaseNeedsUpgrade()
        {

            this.userDatabaseVersion = Convert.ToInt32(DbConnection.ExecuteQuery<string>("SELECT Value FROM Configurations WHERE Key = 'DatabaseVersion'"));

            return this.userDatabaseVersion < CURRENT_VERSION;
        }

        public void InitializeNewDatabase()
        {
            SQLiteConnection.CreateFile(DbConnection.DatabaseFile); // Create the database (file)

            this.connection.Open();
            this.CreateConfiguration();
            this.CreateTablesAndIndexes();
            this.connection.Close();

            LogClient.Instance.Logger.Info("New database created at {0}", DbConnection.DatabaseFile);
        }

        public void UpgradeDatabase()
        {
            this.connection.Open();

            MethodInfo[] methods = typeof(DbCreator).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = this.userDatabaseVersion + 1; i <= CURRENT_VERSION; i++)
            {
                foreach (MethodInfo method in methods)
                {
                    foreach (DatabaseVersionAttribute attr in method.GetCustomAttributes(typeof(DatabaseVersionAttribute), false))
                    {
                        if (attr.Version == i)
                        {
                            method.Invoke(this, null);
                        }
                    }
                }
            }

            this.Execute(string.Format("UPDATE Configurations SET Value = {0} WHERE Key = 'DatabaseVersion'", CURRENT_VERSION));
            this.connection.Close();

            LogClient.Instance.Logger.Info("Migrated from database version {0} to {1}", this.userDatabaseVersion, CURRENT_VERSION);
        }
        #endregion

        #region Private
        private void Execute(string iQuery)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd = this.connection.CreateCommand();
            cmd.CommandText = iQuery;
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        #endregion
    }
}
