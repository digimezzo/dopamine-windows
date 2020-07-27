using Digimezzo.Foundation.Core.Logging;
using Dopamine.Data.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dopamine.Data
{
    public class DbMigrator
    {
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
      
        // NOTE: whenever there is a change in the database schema,
        // this version MUST be incremented and a migration method
        // MUST be supplied to match the new version number
        protected const int CURRENT_VERSION = 27;
        private ISQLiteConnectionFactory factory;
        private int userDatabaseVersion;

        public DbMigrator(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        private void CreateConfiguration()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE Configuration (" +
                             "ConfigurationID    INTEGER," +
                             "Key                TEXT," +
                             "Value              TEXT," +
                             "PRIMARY KEY(ConfigurationID));");

                conn.Execute(String.Format("INSERT INTO Configuration (ConfigurationID, Key, Value) VALUES (null,'DatabaseVersion', {0});", CURRENT_VERSION));
            }
        }

        private void CreateTablesAndIndexes()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE Folder (" +
                             "FolderID	         INTEGER PRIMARY KEY AUTOINCREMENT," +
                             "Path	             TEXT," +
                             "SafePath	         TEXT," +
                             "ShowInCollection   INTEGER);");

                conn.Execute("CREATE TABLE Track (" +
                             "TrackID	                INTEGER," +
                             "Artists	                TEXT," +
                             "Genres	                TEXT," +
                             "AlbumTitle	            TEXT," +
                             "AlbumArtists	            TEXT," +
                             "AlbumKey	                TEXT," +
                             "Path	                    TEXT," +
                             "SafePath	                TEXT," +
                             "FileName	                TEXT," +
                             "MimeType	                TEXT," +
                             "FileSize	                INTEGER," +
                             "BitRate	                INTEGER," +
                             "SampleRate	            INTEGER," +
                             "TrackTitle	            TEXT," +
                             "TrackNumber	            INTEGER," +
                             "TrackCount	            INTEGER," +
                             "DiscNumber	            INTEGER," +
                             "DiscCount	                INTEGER," +
                             "Duration	                INTEGER," +
                             "Year	                    INTEGER," +
                             "HasLyrics	                INTEGER," +
                             "DateAdded  	            INTEGER," +
                             "DateFileCreated  	        INTEGER," +
                             "DateLastSynced	        INTEGER," +
                             "DateFileModified	        INTEGER," +
                             "NeedsIndexing 	        INTEGER," +
                             "NeedsAlbumArtworkIndexing INTEGER," +
                             "IndexingSuccess 	        INTEGER," +
                             "IndexingFailureReason     TEXT," +
                             "Rating	            INTEGER," +
                             "Love	                INTEGER," +
                             "PlayCount	            INTEGER," +
                             "SkipCount	            INTEGER," +
                             "DateLastPlayed        INTEGER," +
                             "PRIMARY KEY(TrackID));");

                conn.Execute("CREATE INDEX TrackPathIndex ON Track(Path);");
                conn.Execute("CREATE INDEX TrackSafePathIndex ON Track(SafePath);");

                conn.Execute("CREATE TABLE AlbumArtwork (" +
                             "AlbumArtworkID	INTEGER," +
                             "AlbumKey	        TEXT," +
                             "ArtworkID	        TEXT," +
                             "PRIMARY KEY(AlbumArtworkID));");

                conn.Execute("CREATE TABLE FolderTrack (" +
                             "FolderTrackID      INTEGER PRIMARY KEY AUTOINCREMENT, " +
                             "FolderID	         INTEGER," +
                             "TrackID	         INTEGER);");

                conn.Execute("CREATE TABLE RemovedTrack (" +
                             "TrackID	            INTEGER," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
                             "DateRemoved           INTEGER," +
                             "PRIMARY KEY(TrackID));");

                conn.Execute("CREATE TABLE QueuedTrack (" +
                             "QueuedTrackID         INTEGER," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
                             "IsPlaying             INTEGER," +
                             "ProgressSeconds       INTEGER," +
                             "OrderID               INTEGER," +
                             "PRIMARY KEY(QueuedTrackID));");

                conn.Execute("CREATE TABLE TrackStatistic (" +
                             "TrackStatisticID	    INTEGER PRIMARY KEY AUTOINCREMENT," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
                             "Rating	            INTEGER," +
                             "Love	                INTEGER," +
                             "PlayCount	            INTEGER," +
                             "SkipCount	            INTEGER," +
                             "DateLastPlayed        INTEGER);");

                conn.Execute("CREATE INDEX TrackStatisticSafePathIndex ON Track(SafePath);");
            }
            CreateTablesAndIndexes_v2();
        }

        private void CreateTablesAndIndexes_v2()
        {
            using (var conn = this.factory.GetConnection())
            {

                conn.Execute("DROP TABLE IF EXISTS History;");
                conn.Execute("DROP TABLE IF EXISTS HistoryActions;");

                conn.Execute("DROP TABLE IF EXISTS TrackArtists;");
                conn.Execute("DROP TABLE IF EXISTS TrackAlbums;");
                conn.Execute("DROP TABLE IF EXISTS TrackLyrics;");
                conn.Execute("DROP TABLE IF EXISTS TrackGenres;");
                conn.Execute("DROP TABLE IF EXISTS Tracks;");

                conn.Execute("DROP TABLE IF EXISTS GenreImages;");
                conn.Execute("DROP TABLE IF EXISTS Genres;");

                conn.Execute("DROP TABLE IF EXISTS AlbumReviews;");
                conn.Execute("DROP TABLE IF EXISTS AlbumImages;");
                conn.Execute("DROP TABLE IF EXISTS Albums;");

                conn.Execute("DROP TABLE IF EXISTS ArtistBiographies;");
                conn.Execute("DROP TABLE IF EXISTS ArtistImages;");
                conn.Execute("DROP TABLE IF EXISTS Artists;");
                conn.Execute("DROP TABLE IF EXISTS ArtistRoles;");

                conn.Execute("DROP TABLE IF EXISTS Folders;");


                //=== Artists:
                conn.Execute("CREATE TABLE Artists (" +
                            "id                 INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "name               TEXT NOT NULL COLLATE NOCASE);");

                conn.Execute("CREATE UNIQUE INDEX ArtistsNameIndex ON Artists(name);");

                //=== ArtistBiographies: (Many 2 many) Each Artist may have multiple biographies
                conn.Execute("CREATE TABLE ArtistBiographies (" +
                            "artist_id          INTEGER," +
                            "biography          TEXT NOT NULL," +
                            "source             TEXT," +
                            "language           TEXT," +
                            "priority           INTEGER NOT NULL DEFAULT 0," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (artist_id) REFERENCES Artists(id));");

                conn.Execute("CREATE INDEX ArtistBiographiesArtistIDIndex ON ArtistBiographies(artist_id);");

                //=== ArtistImages: (Many 2 many) Each Artist may have multiple images
                conn.Execute("CREATE TABLE ArtistImages (" +
                            "artist_id          INTEGER," +
                            "key                TEXT NOT NULL," + 
                            "source             TEXT," +
                            "priority           INTEGER NOT NULL DEFAULT 0," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (artist_id) REFERENCES Artists(id));");

                conn.Execute("CREATE INDEX ArtistImagesArtistIDIndex ON ArtistImages(artist_id);");

                //=== Albums:
                conn.Execute("CREATE TABLE Albums (" +
                            "id                 INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "artist_id          INTEGER," + // If artist_id is null then this album is a collection
                            "name               TEXT NOT NULL COLLATE NOCASE," +
                            "FOREIGN KEY (artist_id) REFERENCES Artists(id)); ");

                conn.Execute("CREATE INDEX AlbumsArtistIDIndex ON Albums(artist_id);");
                conn.Execute("CREATE INDEX AlbumsNameIndex ON Albums(name);");
                conn.Execute("CREATE UNIQUE INDEX AlbumsArtistIDNameIndex ON Albums(artist_id, name);");

                //=== AlbumReviews: (Many 2 many) Each album may have multiple reviews
                conn.Execute("CREATE TABLE AlbumReviews (" +
                            "album_id           INTEGER," +
                            "review             TEXT NOT NULL," +
                            "source             TEXT," +
                            "language           TEXT," +
                            "priority           INTEGER NOT NULL DEFAULT 0," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (album_id) REFERENCES Albums(id));");

                conn.Execute("CREATE INDEX AlbumReviewsArtistIDIndex ON AlbumReviews(album_id);");

                //=== AlbumImages: (Many 2 many) Each album may have multiple images
                conn.Execute("CREATE TABLE AlbumImages (" +
                            "album_id           INTEGER," +
                            "key                TEXT NOT NULL," +
                            "source             TEXT," +
                            "priority           INTEGER NOT NULL DEFAULT 0," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (album_id) REFERENCES Albums(id));");

                conn.Execute("CREATE INDEX AlbumImagesArtistIDIndex ON AlbumImages(album_id);");

                //=== Genres:
                conn.Execute("CREATE TABLE Genres (" +
                            "id                 INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "name               TEXT NOT NULL COLLATE NOCASE);");

                conn.Execute("CREATE UNIQUE INDEX GenresNameIndex ON Genres(name);");

                //=== GenreImages: (Many 2 many) Each genre may have multiple images
                conn.Execute("CREATE TABLE GenreImages (" +
                            "genre_id           INTEGER," +
                            "key                TEXT NOT NULL," +
                            "source             TEXT," +
                            "priority           INTEGER NOT NULL DEFAULT 0," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (genre_id) REFERENCES Genres(id));");

                conn.Execute("CREATE INDEX GenreImagesArtistIDIndex ON GenreImages(genre_id);");

                //=== Folders:
                conn.Execute("CREATE TABLE Folders (" +
                            "id                     INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "path                   TEXT," +
                            "show                   INTEGER DEFAULT 1);");

                conn.Execute("CREATE UNIQUE INDEX FoldersPath ON Folders(path);");

                //=== Tracks:
                conn.Execute("CREATE TABLE Tracks (" +
                            "id	                INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "name	            TEXT NOT NULL COLLATE NOCASE," +
                            "path               TEXT NOT NULL," +
                            "folder_id          INTEGER NOT NULL," +
                            "filesize           INTEGER," +
                            "bitrate            INTEGER," +
                            "samplerate	        INTEGER," +
                            "duration           INTEGER," +
                            "year	            INTEGER," +
                            "language           TEXT," +
                            "date_added	        INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "date_deleted	    INTEGER," +
                            "rating	            INTEGER," +
                            "love	            INTEGER," +
                            "FOREIGN KEY (folder_id) REFERENCES Folders(id));");

                conn.Execute("CREATE INDEX TracksNameIndex ON Tracks(name);");
                conn.Execute("CREATE UNIQUE INDEX TracksPathIndex ON Tracks(path);");
                conn.Execute("CREATE INDEX TracksFolderIDIndex ON Tracks(folder_id);");

                //=== ArtistRoles:
                conn.Execute("CREATE TABLE ArtistRoles (" +
                            "id                 INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "name               TEXT NOT NULL);");

                conn.Execute("INSERT INTO ArtistRoles (name) VALUES ('Composer'), ('Producer'), ('Mixing');");

                //=== TrackArtists: (Many 2 Many) Many Tracks may belong to the same Artist. Many Artists may have collaborate to the same track
                conn.Execute("CREATE TABLE TrackArtists (" +
                            "track_id           INTEGER NOT NULL," +
                            "artist_id          INTEGER NOT NULL," +
                            "artist_role_id     INTEGER DEFAULT 1," +
                            "FOREIGN KEY (track_id) REFERENCES Tracks(id)," +
                            "FOREIGN KEY (artist_id) REFERENCES Artists(id)," +
                            "FOREIGN KEY (artist_role_id) REFERENCES ArtistRoles(id)); ");

                conn.Execute("CREATE INDEX TrackArtistsTrackIDIndex ON TrackArtists(track_id);");
                conn.Execute("CREATE INDEX TrackArtistsArtistIDIndex ON TrackArtists(artist_id);");
                conn.Execute("CREATE INDEX TrackArtistsArtistRoleIDIndex ON TrackArtists(artist_role_id);");
                conn.Execute("CREATE UNIQUE INDEX TrackArtistsCombinedIndex ON TrackArtists(track_id, artist_id, artist_role_id);");

                //=== TrackArtists: (Many 2 Many) Many Tracks may belong to the same Artist. Many Artists may have collaborate to the same track
                conn.Execute("CREATE TABLE TrackGenres (" +
                            "track_id           INTEGER NOT NULL," +
                            "genre_id           INTEGER NOT NULL," +
                            "FOREIGN KEY (track_id) REFERENCES Tracks(id)," +
                            "FOREIGN KEY (genre_id) REFERENCES Genres(id)); ");

                conn.Execute("CREATE INDEX TrackGenresTrackIDIndex ON TrackGenres(track_id);");
                conn.Execute("CREATE INDEX TrackGenresArtistIDIndex ON TrackGenres(genre_id);");
                conn.Execute("CREATE UNIQUE INDEX TrackGenresCombinedIndex ON TrackGenres(track_id, genre_id);");

                //=== TrackAlbum: (Many 2 Many) Each Track may have zero or one TrackAlbum record 
                conn.Execute("CREATE TABLE TrackAlbums (" +
                            "track_id           INTEGER NOT NULL," +
                            "album_id           INTEGER NOT NULL," +
                            "track_number       INTEGER," +
                            "disc_number        INTEGER," +
                            "FOREIGN KEY (track_id) REFERENCES Tracks(id)," +
                            "FOREIGN KEY (album_id) REFERENCES Albums(id)); ");

                conn.Execute("CREATE INDEX TrackAlbumTrackIDIndex ON TrackAlbums(track_id);");
                conn.Execute("CREATE INDEX TrackAlbumAlbumIDIndex ON TrackAlbums(album_id);");
                conn.Execute("CREATE UNIQUE INDEX TrackAlbumsCombinedIndex ON TrackAlbums(track_id, album_id);");

                //=== TrackLyrics: (One 2 One) Each Track may have zero or one Lyrics record 
                conn.Execute("CREATE TABLE TrackLyrics (" +
                            "track_id           INTEGER," +
                            "lyrics             TEXT NOT NULL COLLATE NOCASE," +
                            "source             TEXT," +
                            "language           TEXT," +
                            "date_added         INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (track_id) REFERENCES Tracks(id));");

                conn.Execute("CREATE INDEX TrackLyricsLyricsIndex ON TrackLyrics(lyrics);");

                //=== HistoryActions: Should include Added, Deleted, Modified, Played, AutoPlayed, Skipped, Rated, Loved
                conn.Execute("CREATE TABLE HistoryActions (" +
                            "id                 INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "name               TEXT NOT NULL);");

                conn.Execute("INSERT INTO HistoryActions (name) VALUES ('Added'), ('Removed'), ('Modified'), ('Played'), ('Skipped'), ('Rated'), ('Loved');");


                //=== History:
                conn.Execute("CREATE TABLE History (" +
                            "id                     INTEGER PRIMARY KEY AUTOINCREMENT," +
                            "track_id               INTEGER NOT NULL," +
                            "history_action_id      INTEGER NOT NULL," +
                            "history_action_extra   TEXT," +
                            "date_happened          INTEGER NOT NULL DEFAULT CURRENT_TIMESTAMP," +
                            "FOREIGN KEY (track_id) REFERENCES Tracks(id)," +
                            "FOREIGN KEY (history_action_id) REFERENCES HistoryActions(id));");

                conn.Execute("CREATE INDEX HistoryTrackIDIndex ON History(track_id);");
                conn.Execute("CREATE INDEX HistoryHistoryActionIDIndex ON History(history_action_id);");

                List<Folder> folders = conn.Table<Folder>().ToList();
                foreach (Folder folder in folders)
                {
                    Console.WriteLine("Migrating Folder {0}", folder.Path);
                    conn.Insert(new Folder2() { Path = folder.Path});
                }

                // Get all the items from "track" table. Add them to the new structure

                string query = @"SELECT DISTINCT t.TrackID, t.Artists, t.Genres, t.AlbumTitle, t.AlbumArtists, t.AlbumKey,
                    t.Path, t.SafePath, t.FileName, t.MimeType, t.FileSize, t.BitRate, 
                    t.SampleRate, t.TrackTitle, t.TrackNumber, t.TrackCount, t.DiscNumber,
                    t.DiscCount, t.Duration, t.Year, t.HasLyrics, t.DateAdded, t.DateFileCreated,
                    t.DateLastSynced, t.DateFileModified, t.NeedsIndexing, t.NeedsAlbumArtworkIndexing, t.IndexingSuccess,
                    t.IndexingFailureReason, t.Rating, t.Love, t.PlayCount, t.SkipCount, t.DateLastPlayed
                    FROM Track t
                    INNER JOIN FolderTrack ft ON ft.TrackID = t.TrackID
                    INNER JOIN Folder f ON ft.FolderID = f.FolderID
                    WHERE f.ShowInCollection = 1 AND t.IndexingSuccess = 1 AND t.NeedsIndexing = 0";

                //var tracks = new List<Track>();
                List<Track> tracks = conn.Query<Track>(query);

                foreach (Track track in tracks)
                {
                    Console.WriteLine("Migrating File {0}", track.Path);
                    List<FolderTrack> folderTrackIDs = conn.Query<FolderTrack>(@"SELECT * FROM FolderTrack WHERE TrackID=?", track.TrackID);
                    conn.Insert(new Track2()
                    {
                        Name = track.TrackTitle,
                        Path = track.Path,
                        FolderId = folderTrackIDs[0].FolderID,
                        Filesize = track.FileSize,
                        Bitrate = track.BitRate,
                        Samplerate = track.SampleRate,
                        Duration = track.Duration,
                        Year = track.Year,
                        Language = null,
                        DateAdded = track.DateAdded,
                        Rating = track.Rating,
                        Love = track.Love
                    });
                    long track_id = GetLastInsertRowID(conn);
                    Console.WriteLine("--> Created Tracks.ID: {0}", track_id);

                    //Add the artists
                    long artistID = 0;
                    if (track.Artists.Length > 0)
                    {
                        string[] artists = track.Artists.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                        for (int i = 0; i < artists.Length; i++)
                        {
                            long curID = GetArtistID(conn, artists[i]);
                            Console.WriteLine("Artist: {0}->{1}", curID, artists[i]);
                            conn.Insert(new TrackArtist()
                            {
                                TrackId = track_id,
                                ArtistId = curID,
                                ArtistRoleId = 1
                            });
                        if (artists.Length == 1)
                                artistID = curID;
                        }
                    }
                    if (track.AlbumTitle.Length > 0)
                    {
                        long albumID = GetAlbumID(conn, track.AlbumTitle, artistID);
                        conn.Insert(new TrackAlbum()
                        {
                            TrackId = track_id,
                            AlbumId = albumID,
                            TrackNumber = track.TrackNumber,
                            DiscNumber = track.DiscNumber
                        });
                    }



                if (track.Genres.Length > 0)
                    {
                        string[] genres = track.Genres.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                        for (int i = 0; i < genres.Length; i++)
                        {
                            long curID = GetGenreID(conn, genres[i]);
                            try
                            {
                                conn.Insert(new TrackGenre()
                                {
                                    TrackId = track_id,
                                    GenreId = curID
                                });
                            }
                            catch (SQLite.SQLiteException ex)
                            {
                                Console.WriteLine("SQLiteException (Genres) {0}", ex.Message);
                            }

                        }
                    }

                }




                //=== TODO
                // Check if we can have all the queries that we need
                // Check the album art etc.
                // Create the repositories
                // Replace the old db handling
                // Create a proper migration

                /* SAMPLE QUERIES
----- GET ALL TRACKS
SELECT t.name as title, GROUP_CONCAT(Artists.name,"|") as artist, GROUP_CONCAT(Albums.name,"|") as album, GROUP_CONCAT(Genres.name,"|") as genre,t."path" 
from Tracks t 
LEFT JOIN TrackArtists ON TrackArtists.track_id =t.id 
LEFT JOIN Artists ON Artists.id =TrackArtists.artist_id  
LEFT JOIN TrackAlbums ON TrackAlbums.track_id =t.id 
LEFT JOIN Albums ON Albums.id =TrackAlbums.album_id  
LEFT JOIN TrackGenres ON TrackGenres.track_id =t.id 
LEFT JOIN Genres ON Genres.id =TrackGenres.genre_id  
GROUP BY t.id 
having count(Artists.id) > 1
*** PROBLEM: if there are two of a kind it duplicates everything eg: Stay Around	Wipers|Wipers	The Power In One|The Power In One	Punk|Alternative

----- GET ALL ARTISTS
SELECT Artists.name, COUNT(t.id) 
from Tracks t
LEFT JOIN TrackArtists  ON TrackArtists.track_id =t.id 
LEFT JOIN Artists ON Artists.id =TrackArtists.artist_id  
GROUP BY Artists.id
ORDER BY Artists.name

----- GET THE ALBUMS OF AN ARTIST
SELECT Albums.name, COUNT(t.id) 
from Tracks t
LEFT JOIN TrackAlbums  ON TrackAlbums.track_id =t.id 
LEFT JOIN Albums  ON TrackAlbums.album_id =Albums.id 
LEFT JOIN TrackArtists  ON TrackArtists.track_id =t.id 
LEFT JOIN Artists ON Artists.id =TrackArtists.artist_id  
WHERE Artists.name="Black Angels"
GROUP BY Albums.id
ORDER BY Albums.name

----- GET ALL GENRES OF AN ARTIST
SELECT Genres.name, COUNT(t.id) 
from Tracks t
LEFT JOIN TrackGenres  ON TrackGenres.track_id =t.id 
LEFT JOIN Genres  ON TrackGenres.genre_id=Genres.id 
LEFT JOIN TrackArtists  ON TrackArtists.track_id =t.id 
LEFT JOIN Artists ON Artists.id =TrackArtists.artist_id  
WHERE Artists.name="Black Angels"
GROUP BY Genres.id
ORDER BY Genres.name

***** PROBLEM: "Judgment Night" is a collection but it appears as many different albums. Do we need the "Artist ID" in "Albums"?



                */






            }
        }

        private long GetArtistID(SQLiteConnection conn, String entry)
        {
            List<long> ids = conn.QueryScalars<long>("SELECT * FROM Artists WHERE name=?", entry);
            if (ids.Count == 0)
            {
                conn.Insert(new Artist() { Name = entry });
                return GetLastInsertRowID(conn);
            }
            return ids[0];
        }
        private long GetAlbumID(SQLiteConnection conn, String entry, long artistID)
        {
            List<long> ids;
            if (artistID == 0)
            {
                ids = conn.QueryScalars<long>("SELECT * FROM Albums WHERE name=? AND artist_id is NULL", entry);
                if (ids.Count == 0)
                {
                    conn.Insert(new Album() { Name = entry });
                    return GetLastInsertRowID(conn);
                }
            }
            else
            {
                ids = conn.QueryScalars<long>("SELECT * FROM Albums WHERE name=? AND artist_id=?", entry, artistID);
                if (ids.Count == 0)
                {
                    conn.Insert(new Album() { Name = entry, ArtistID = artistID });
                    return GetLastInsertRowID(conn);
                }
            }
            return ids[0];
        }
        private long GetGenreID(SQLiteConnection conn, String entry)
        {
            List<long> ids = conn.QueryScalars<long>("SELECT * FROM Genres WHERE name=?", entry);
            if (ids.Count == 0)
            {
                conn.Insert(new Genre() { Name = entry });
                return GetLastInsertRowID(conn);
            }
            return ids[0];
        }

        private long GetLastInsertRowID(SQLiteConnection conn)
        {
            SQLiteCommand cmdLastRow = conn.CreateCommand(@"select last_insert_rowid()");
            return cmdLastRow.ExecuteScalar<long>();
        }

        [DatabaseVersion(1)]
        private void Migrate1()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("ALTER TABLE Tracks ADD DiscNumber INTEGER;");
                conn.Execute("ALTER TABLE Tracks ADD DiscCount INTEGER;");

                conn.Execute("UPDATE Tracks SET DiscNumber=(SELECT DiscNumber FROM Albums WHERE Albums.AlbumID=Tracks.AlbumID);");
                conn.Execute("UPDATE Tracks SET DiscCount=(SELECT DiscCount FROM Albums WHERE Albums.AlbumID=Tracks.AlbumID);");

                conn.Execute("CREATE TABLE Albums_Backup (" +
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

                conn.Execute("INSERT INTO Albums_Backup SELECT AlbumID," +
                             "AlbumTitle," +
                             "AlbumArtist," +
                             "EmbeddedArtworkID," +
                             "EmbeddedArtworkSize," +
                             "ExternalArtworkID," +
                             "ExternalArtworkSize," +
                             "ExternalArtworkPath," +
                             "ExternalArtworkDateFileModified, (SELECT AlbumID FROM Albums ab WHERE LOWER(TRIM(a.AlbumTitle))=LOWER(TRIM(ab.AlbumTitle)) AND LOWER(TRIM(a.AlbumArtist))=LOWER(TRIM(ab.AlbumArtist)) ORDER BY AlbumID LIMIT 1) " +
                             "FROM Albums a;");

                conn.Execute("UPDATE Tracks SET AlbumID=(SELECT First_AlbumID FROM Albums_Backup WHERE Albums_Backup.AlbumID=Tracks.AlbumID);");
                conn.Execute("DROP TABLE Albums;");

                conn.Execute("CREATE TABLE Albums (" +
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

                conn.Execute("INSERT INTO Albums SELECT AlbumID," +
                             "AlbumTitle," +
                             "AlbumArtist," +
                             "EmbeddedArtworkID," +
                             "EmbeddedArtworkSize," +
                             "ExternalArtworkID," +
                             "ExternalArtworkSize," +
                             "ExternalArtworkPath," +
                             "ExternalArtworkDateFileModified " +
                             "FROM Albums_Backup WHERE AlbumID=First_AlbumID;");

                conn.Execute("DROP TABLE Albums_Backup;");

                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(2)]
        private void Migrate2()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE Albums_Backup (" +
                             "AlbumID	        INTEGER," +
                             "AlbumTitle	    TEXT," +
                             "AlbumArtist	    TEXT," +
                             "PRIMARY KEY(AlbumID));");

                conn.Execute("INSERT INTO Albums_Backup SELECT AlbumID," +
                             "AlbumTitle," +
                             "AlbumArtist " +
                             "FROM Albums;");

                conn.Execute("DROP TABLE Albums;");

                conn.Execute("CREATE TABLE Albums (" +
                             "AlbumID	        INTEGER," +
                             "AlbumTitle	    TEXT," +
                             "AlbumArtist	    TEXT," +
                             "Year	            INTEGER," +
                             "ArtworkID	        TEXT," +
                             "DateLastSynced	INTEGER," +
                             "PRIMARY KEY(AlbumID));");

                conn.Execute("INSERT INTO Albums SELECT AlbumID," +
                             "AlbumTitle," +
                             "AlbumArtist," +
                             "0," +
                             "null," +
                             "0 " +
                             "FROM Albums_Backup;");

                conn.Execute("DROP TABLE Albums_Backup;");

                conn.Execute("CREATE INDEX IF NOT EXISTS TracksFolderIDIndex ON Tracks(FolderID);");
                conn.Execute("CREATE INDEX IF NOT EXISTS TracksArtistIDIndex ON Tracks(ArtistID);");
                conn.Execute("CREATE INDEX IF NOT EXISTS TracksAlbumIDIndex ON Tracks(AlbumID);");
                conn.Execute("CREATE INDEX IF NOT EXISTS TracksPathIndex ON Tracks(Path);");
                conn.Execute("CREATE INDEX IF NOT EXISTS ArtistsIndex ON Artists(ArtistName);");
                conn.Execute("CREATE INDEX IF NOT EXISTS AlbumsIndex ON Albums(AlbumTitle, AlbumArtist);");

                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(3)]
        private void Migrate3()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE RemovedTracks (" +
                             "TrackID	            INTEGER," +
                             "Path	                TEXT," +
                             "DateRemoved           INTEGER," +
                             "PRIMARY KEY(TrackID));");

                conn.Execute("BEGIN TRANSACTION;");
                conn.Execute("CREATE TEMPORARY TABLE Tracks_Backup (" +
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

                conn.Execute("INSERT INTO Tracks_Backup SELECT TrackID," +
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

                conn.Execute("DROP TABLE Tracks;");

                conn.Execute("CREATE TABLE Tracks (" +
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

                conn.Execute("INSERT INTO Tracks SELECT TrackID," +
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

                conn.Execute("DROP TABLE Tracks_Backup;");

                conn.Execute("COMMIT;");

                conn.Execute("CREATE INDEX IF NOT EXISTS TracksArtistIDIndex ON Tracks(ArtistID);");
                conn.Execute("CREATE INDEX IF NOT EXISTS TracksAlbumIDIndex ON Tracks(AlbumID);");
                conn.Execute("CREATE INDEX TracksPathIndex ON Tracks(Path)");

                conn.Execute("ALTER TABLE Albums ADD DateAdded INTEGER;");
                conn.Execute("UPDATE Albums SET DateAdded=(SELECT MIN(DateAdded) FROM Tracks WHERE Tracks.AlbumID = Albums.AlbumID);");

                conn.Execute("VACUUM;");
            }
        }
       
        [DatabaseVersion(4)]
        private void Migrate4()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE Genres (" +
                             "GenreID           INTEGER," +
                             "GenreName	        TEXT," +
                             "PRIMARY KEY(GenreID));");

                conn.Execute("ALTER TABLE Tracks ADD GenreID INTEGER;");

                conn.Execute("INSERT INTO Genres(GenreName) SELECT DISTINCT Genre FROM Tracks WHERE TRIM(Genre) <>'';");
                conn.Execute("UPDATE Tracks SET GenreID=(SELECT GenreID FROM Genres WHERE Genres.GenreName=Tracks.Genre) WHERE TRIM(Genre) <> '';");

                conn.Execute("INSERT INTO Genres(GenreName) VALUES('%unknown_genre%');");
                conn.Execute("UPDATE Tracks SET GenreID=(SELECT GenreID FROM Genres WHERE Genres.GenreName='%unknown_genre%') WHERE TRIM(Genre) = '';");

                conn.Execute("CREATE TABLE Tracks_Backup (" +
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

                conn.Execute("INSERT INTO Tracks_Backup SELECT TrackID," +
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

                conn.Execute("DROP TABLE Tracks;");

                conn.Execute("CREATE TABLE Tracks (" +
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

                conn.Execute("INSERT INTO Tracks SELECT TrackID," +
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

                conn.Execute("DROP TABLE Tracks_Backup;");

                conn.Execute("CREATE INDEX IF NOT EXISTS TracksGenreIDIndex ON Tracks(GenreID);");
                conn.Execute("CREATE INDEX GenresIndex ON Genres(GenreName);");
            }
        }
      
        [DatabaseVersion(5)]
        private void Migrate5()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("UPDATE Albums SET Year=(SELECT MAX(Year) FROM Tracks WHERE Tracks.AlbumID=Albums.AlbumID) WHERE AlbumTitle<>'Unknown Album';");
                conn.Execute("CREATE INDEX IF NOT EXISTS AlbumsYearIndex ON Albums(Year);");
            }
        }
   
        [DatabaseVersion(6)]
        private void Migrate6()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("ALTER TABLE Tracks ADD FolderID INTEGER;");
                conn.Execute("UPDATE Tracks SET FolderID=(SELECT FolderID FROM Folders WHERE UPPER(Tracks.Path) LIKE UPPER(Folders.Path)||'%');");

                conn.Execute("CREATE INDEX IF NOT EXISTS TracksFolderIDIndex ON Tracks(FolderID);");
            }
        }
    
        [DatabaseVersion(7)]
        private void Migrate7()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("ALTER TABLE Folders ADD ShowInCollection INTEGER;");
                conn.Execute("UPDATE Folders SET ShowInCollection=1;");
            }
        }
     
        [DatabaseVersion(8)]
        private void Migrate8()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE QueuedTracks (" +
                             "QueuedTrackID     INTEGER," +
                             "Path	             TEXT," +
                             "OrderID           INTEGER," +
                             "PRIMARY KEY(QueuedTrackID));");
            }
        }
       
        [DatabaseVersion(9)]
        private void Migrate9()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("CREATE TABLE IndexingStatistics (" +
                             "IndexingStatisticID    INTEGER," +
                             "Key                    TEXT," +
                             "Value                  TEXT," +
                             "PRIMARY KEY(IndexingStatisticID));");
            }
        }
     
        [DatabaseVersion(10)]
        private void Migrate10()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("CREATE TABLE Folders_backup (" +
                             "FolderID	         INTEGER," +
                             "Path	             TEXT," +
                             "ShowInCollection   INTEGER," +
                             "PRIMARY KEY(FolderID));");

                conn.Execute("INSERT INTO Folders_backup SELECT * FROM Folders;");

                conn.Execute("DROP TABLE Folders;");

                conn.Execute("CREATE TABLE Folders (" +
                             "FolderID	         INTEGER PRIMARY KEY AUTOINCREMENT," +
                             "Path	             TEXT," +
                             "ShowInCollection   INTEGER);");

                conn.Execute("INSERT INTO Folders SELECT * FROM Folders_backup;");

                conn.Execute("DROP TABLE Folders_backup;");

                conn.Execute("COMMIT;");

                conn.Execute("VACUUM;");
            }
        }
   
        [DatabaseVersion(11)]
        private void Migrate11()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("DROP INDEX IF EXISTS ArtistsIndex;");
                conn.Execute("DROP INDEX IF EXISTS GenresIndex;");
                conn.Execute("DROP INDEX IF EXISTS AlbumsIndex;");
                conn.Execute("DROP INDEX IF EXISTS AlbumsYearIndex;");
                conn.Execute("DROP INDEX IF EXISTS TracksArtistIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TracksAlbumIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TracksGenreIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TracksFolderIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TracksPathIndex;");

                conn.Execute("ALTER TABLE Configurations RENAME TO Configuration;");
                conn.Execute("ALTER TABLE Artists RENAME TO Artist;");
                conn.Execute("ALTER TABLE Genres RENAME TO Genre;");
                conn.Execute("ALTER TABLE Albums RENAME TO Album;");
                conn.Execute("ALTER TABLE Playlists RENAME TO Playlist;");
                conn.Execute("ALTER TABLE PlaylistEntries RENAME TO PlaylistEntry;");
                conn.Execute("ALTER TABLE Folders RENAME TO Folder;");
                conn.Execute("ALTER TABLE Tracks RENAME TO Track;");
                conn.Execute("ALTER TABLE RemovedTracks RENAME TO RemovedTrack;");
                conn.Execute("ALTER TABLE QueuedTracks RENAME TO QueuedTrack;");
                conn.Execute("ALTER TABLE IndexingStatistics RENAME TO IndexingStatistic;");

                conn.Execute("CREATE INDEX ArtistIndex ON Artist(ArtistName)");
                conn.Execute("CREATE INDEX GenreIndex ON Genre(GenreName)");
                conn.Execute("CREATE INDEX AlbumIndex ON Album(AlbumTitle, AlbumArtist)");
                conn.Execute("CREATE INDEX AlbumYearIndex ON Album(Year);");
                conn.Execute("CREATE INDEX TrackArtistIDIndex ON Track(ArtistID);");
                conn.Execute("CREATE INDEX TrackAlbumIDIndex ON Track(AlbumID);");
                conn.Execute("CREATE INDEX TrackGenreIDIndex ON Track(GenreID);");
                conn.Execute("CREATE INDEX TrackFolderIDIndex ON Track(FolderID);");
                conn.Execute("CREATE INDEX TrackPathIndex ON Track(Path)");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
   
        [DatabaseVersion(12)]
        private void Migrate12()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Track ADD SafePath TEXT;");
                conn.Execute("UPDATE Track SET SafePath=LOWER(Path);");

                conn.Execute("CREATE INDEX TrackSafePathIndex ON Track(SafePath);");

                conn.Execute("ALTER TABLE Folder ADD SafePath TEXT;");
                conn.Execute("UPDATE Folder SET SafePath=LOWER(Path);");

                conn.Execute("ALTER TABLE RemovedTrack ADD SafePath TEXT;");
                conn.Execute("UPDATE RemovedTrack SET SafePath=LOWER(Path);");

                conn.Execute("ALTER TABLE QueuedTrack ADD SafePath TEXT;");
                conn.Execute("UPDATE QueuedTrack SET SafePath=LOWER(Path);");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
       
        [DatabaseVersion(13)]
        private void Migrate13()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Track ADD Love INTEGER;");
                conn.Execute("UPDATE Track SET Love=0;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
   
        [DatabaseVersion(14)]
        private void Migrate14()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE QueuedTrack ADD IsPlaying INTEGER;");
                conn.Execute("ALTER TABLE QueuedTrack ADD ProgressSeconds INTEGER;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
   
        [DatabaseVersion(15)]
        private void Migrate15()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Track ADD HasLyrics INTEGER;");
                conn.Execute("ALTER TABLE Track ADD NeedsIndexing INTEGER;");
                conn.Execute("UPDATE Track SET HasLyrics=0;");
                conn.Execute("UPDATE Track SET NeedsIndexing=1;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
  
        [DatabaseVersion(16)]
        private void Migrate16()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("CREATE TABLE TrackStatistic (" +
                             "TrackStatisticID	    INTEGER PRIMARY KEY AUTOINCREMENT," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
                             "Rating	            INTEGER," +
                             "Love	                INTEGER," +
                             "PlayCount	            INTEGER," +
                             "SkipCount	            INTEGER," +
                             "DateLastPlayed        INTEGER);");

                conn.Execute("CREATE INDEX TrackStatisticSafePathIndex ON TrackStatistic(SafePath);");

                conn.Execute("INSERT INTO TrackStatistic(Path,SafePath,Rating,Love,PlayCount,SkipCount,DateLastPlayed) " +
                             "SELECT Path, Safepath, Rating, Love, PlayCount,SkipCount, DateLastPlayed FROM Track;");

                conn.Execute("CREATE TEMPORARY TABLE Track_Backup (" +
                             "TrackID	            INTEGER," +
                             "ArtistID	            INTEGER," +
                             "GenreID	            INTEGER," +
                             "AlbumID	            INTEGER," +
                             "FolderID	            INTEGER," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
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
                             "HasLyrics	            INTEGER," +
                             "DateAdded  	        INTEGER," +
                             "DateLastSynced	    INTEGER," +
                             "DateFileModified	    INTEGER," +
                             "MetaDataHash	        TEXT," +
                             "NeedsIndexing 	    INTEGER," +
                             "PRIMARY KEY(TrackID));");

                conn.Execute("INSERT INTO Track_Backup SELECT TrackID," +
                                     "ArtistID," +
                                     "GenreID," +
                                     "AlbumID," +
                                     "FolderID," +
                                     "Path," +
                                     "SafePath," +
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
                                     "HasLyrics," +
                                     "DateAdded," +
                                     "DateLastSynced," +
                                     "DateFileModified," +
                                     "MetaDataHash," +
                                     "NeedsIndexing " +
                                     "FROM Track;");

                conn.Execute("DROP TABLE Track;");

                conn.Execute("CREATE TABLE Track (" +
                             "TrackID	            INTEGER," +
                             "ArtistID	            INTEGER," +
                             "GenreID	            INTEGER," +
                             "AlbumID	            INTEGER," +
                             "FolderID	            INTEGER," +
                             "Path	                TEXT," +
                             "SafePath	            TEXT," +
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
                             "HasLyrics	            INTEGER," +
                             "DateAdded  	        INTEGER," +
                             "DateLastSynced	    INTEGER," +
                             "DateFileModified	    INTEGER," +
                             "MetaDataHash	        TEXT," +
                             "NeedsIndexing 	    INTEGER," +
                             "PRIMARY KEY(TrackID));");

                conn.Execute("INSERT INTO Track SELECT TrackID," +
                                     "ArtistID," +
                                     "GenreID," +
                                     "AlbumID," +
                                     "FolderID," +
                                     "Path," +
                                     "SafePath," +
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
                                     "HasLyrics," +
                                     "DateAdded," +
                                     "DateLastSynced," +
                                     "DateFileModified," +
                                     "MetaDataHash," +
                                     "NeedsIndexing " +
                                     "FROM Track_Backup;");

                conn.Execute("DROP TABLE Track_Backup;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
 
        [DatabaseVersion(17)]
        private void Migrate17()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("DELETE FROM QueuedTrack;");
                conn.Execute("ALTER TABLE QueuedTrack ADD QueueID TEXT;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
       
        [DatabaseVersion(18)]
        private void Migrate18()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("DROP TABLE Playlist;");
                conn.Execute("DROP TABLE PlaylistEntry;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
   
        [DatabaseVersion(19)]
        private void Migrate19()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Album ADD DateCreated INTEGER;");
                conn.Execute("UPDATE Album SET DateCreated=DateAdded;");
                conn.Execute($"UPDATE Album SET DateAdded={DateTime.Now.Ticks};");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
       
        [DatabaseVersion(20)]
        private void Migrate20()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute($"UPDATE Album SET AlbumTitle='%unknown_album%' WHERE AlbumTitle='Unknown Album';");
                conn.Execute($"UPDATE Album SET AlbumArtist='%unknown_artist%' WHERE AlbumArtist IN ('Unknown Artist','Unknown Album Artist');");
                conn.Execute($"UPDATE Genre SET GenreName='%unknown_genre%' WHERE GenreName='Unknown Genre';");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
    
        [DatabaseVersion(21)]
        private void Migrate21()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("DROP TABLE IndexingStatistic;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }
    
        [DatabaseVersion(22)]
        private void Migrate22()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Track ADD IndexingSuccess INTEGER;");
                conn.Execute("ALTER TABLE Track ADD IndexingFailureReason TEXT;");
                conn.Execute("UPDATE Track SET IndexingSuccess=1;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(23)]
        private void Migrate23()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Album ADD NeedsIndexing INTEGER;");
                conn.Execute("UPDATE Album SET NeedsIndexing=1;");

                conn.Execute("CREATE TABLE FolderTrack (" +
                             "FolderTrackID      INTEGER PRIMARY KEY AUTOINCREMENT, " +
                             "FolderID	         INTEGER," +
                             "TrackID	         INTEGER);");

                conn.Execute("INSERT INTO FolderTrack(FolderID, TrackID) SELECT FolderID,TrackID FROM Track;");

                conn.Execute("UPDATE Track SET FolderID=0;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(24)]
        private void Migrate24()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("DELETE FROM Track WHERE AlbumID = (SELECT AlbumID FROM Album WHERE AlbumTitle IS NULL);");
                conn.Execute("DELETE FROM Artist WHERE ArtistName IS NULL;");
                conn.Execute("DELETE FROM Genre WHERE GenreName IS NULL;");
                conn.Execute("DELETE FROM Album WHERE AlbumTitle IS NULL;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(25)]
        private void Migrate25()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("ALTER TABLE Track ADD Artists TEXT;");
                conn.Execute("ALTER TABLE Track ADD Genres TEXT;");
                conn.Execute("ALTER TABLE Track ADD AlbumTitle TEXT;");
                conn.Execute("ALTER TABLE Track ADD AlbumArtists TEXT;");
                conn.Execute("ALTER TABLE Track ADD AlbumKey TEXT;");
                conn.Execute("ALTER TABLE Track ADD Rating INTEGER;");
                conn.Execute("ALTER TABLE Track ADD Love INTEGER;");
                conn.Execute("ALTER TABLE Track ADD PlayCount INTEGER;");
                conn.Execute("ALTER TABLE Track ADD SkipCount INTEGER;");
                conn.Execute("ALTER TABLE Track ADD DateLastPlayed INTEGER;");
                conn.Execute("ALTER TABLE Track ADD NeedsAlbumArtworkIndexing INTEGER;");
                conn.Execute("ALTER TABLE Track ADD DateFileCreated INTEGER;");

                conn.Execute("UPDATE Track SET Artists='';");
                conn.Execute("UPDATE Track SET Genres='';");
                conn.Execute("UPDATE Track SET AlbumTitle='';");
                conn.Execute("UPDATE Track SET AlbumArtists='';");
                conn.Execute("UPDATE Track SET AlbumKey='';");

                conn.Execute("DROP TABLE Artist;");
                conn.Execute("DROP TABLE Genre;");
                conn.Execute("DROP TABLE Album;");

                conn.Execute("UPDATE Track SET ArtistID=NULL;");
                conn.Execute("UPDATE Track SET GenreID=NULL;");
                conn.Execute("UPDATE Track SET AlbumID=NULL;");
                conn.Execute("UPDATE Track SET MetaDataHash=NULL;");

                conn.Execute("DROP INDEX IF EXISTS TrackArtistIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TrackAlbumIDIndex;");
                conn.Execute("DROP INDEX IF EXISTS TrackGenreIDIndex;");

                conn.Execute("UPDATE Track SET NeedsIndexing=1;");
                conn.Execute("UPDATE Track SET NeedsAlbumArtworkIndexing=1;");

                conn.Execute("CREATE TABLE AlbumArtwork (" +
                             "AlbumArtworkID	INTEGER," +
                             "AlbumKey	        TEXT," +
                             "ArtworkID	        TEXT," +
                             "PRIMARY KEY(AlbumArtworkID));");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }

        [DatabaseVersion(26)]
        private void Migrate26()
        {
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                conn.Execute("UPDATE Track SET PlayCount=0 WHERE PlayCount IS NULL;");
                conn.Execute("UPDATE Track SET SkipCount=0 WHERE SkipCount IS NULL;");

                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
        }


        //=== My Update ====
        [DatabaseVersion(27)]
        private void Migrate27()
        {
            CreateTablesAndIndexes_v2();
            /*
            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("BEGIN TRANSACTION;");

                
                conn.Execute("COMMIT;");
                conn.Execute("VACUUM;");
            }
            */
            }

            public void Migrate()
        {
            try
            {
                if (!this.IsDatabaseValid())
                {
                    // Create the database if it doesn't exist
                    LogClient.Info("Creating a new database");
                    this.CreateDatabase();
                }
                else
                {
                    // Upgrade the database if it is not the latest version
                    if (this.IsMigrationNeeded())
                    {
                        LogClient.Info("Creating a backup of the database");
                        this.BackupDatabase();
                        LogClient.Info("Upgrading database");
                        this.MigrateDatabase();
                    }
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem initializing the database. Exception: {0}", ex.Message);
            }
        }
  
        private bool IsDatabaseValid()
        {
            int count = 0;

            using (var conn = this.factory.GetConnection())
            {
                // HACK: in database version 11, the table Configurations was renamed to Configuration. When migrating from version 10 to 11, 
                // we still need to get the version from the original table as the new Configuration doesn't exist yet and is not found. 
                // At some later point in time, this try catch can be removed.
                count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Configurations'");

                if(count == 0)
                {
                    count = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Configuration'");
                }
            }

            return count > 0;
        }

        public bool IsMigrationNeeded()
        {
            if (!this.IsDatabaseValid())
            {
                return true;
            }

            using (var conn = this.factory.GetConnection())
            {
                try
                {
                    //=== ALEX DEBUG:
                    userDatabaseVersion = 26;
                    //this.userDatabaseVersion = Convert.ToInt32(conn.ExecuteScalar<string>("SELECT Value FROM Configuration WHERE Key = 'DatabaseVersion'"));
                }
                catch (Exception)
                {
                    // HACK: in database version 11, the table Configurations was renamed to Configuration. When migrating from version 10 to 11, 
                    // we still need to get the version from the original table as the new Configuration doesn't exist yet and is not found. 
                    // At some later point in time, this try catch can be removed.
                    this.userDatabaseVersion = Convert.ToInt32(conn.ExecuteScalar<string>("SELECT Value FROM Configurations WHERE Key = 'DatabaseVersion'"));
                }
            }

            return this.userDatabaseVersion < CURRENT_VERSION;
        }

        private void CreateDatabase()
        {
            this.CreateConfiguration();
            this.CreateTablesAndIndexes();

            LogClient.Info("New database created at {0}", this.factory.DatabaseFile);
        }

        private void MigrateDatabase()
        {
            for (int i = this.userDatabaseVersion + 1; i <= CURRENT_VERSION; i++)
            {
                MethodInfo method = typeof(DbMigrator).GetTypeInfo().GetDeclaredMethod("Migrate" + i);
                if (method != null) method.Invoke(this, null);
            }

            using (var conn = this.factory.GetConnection())
            {
                conn.Execute("UPDATE Configuration SET Value = ? WHERE Key = 'DatabaseVersion'", CURRENT_VERSION);
            }

            LogClient.Info("Upgraded from database version {0} to {1}", this.userDatabaseVersion.ToString(), CURRENT_VERSION.ToString());
        }
      
        private void BackupDatabase()
        {
            try
            {
                string databaseFileCopy = this.factory.DatabaseFile + ".old";

                if (File.Exists(databaseFileCopy)) File.Delete(databaseFileCopy);
                File.Copy(this.factory.DatabaseFile, databaseFileCopy);
            }
            catch (Exception ex)
            {
                LogClient.Info("Could not create a copy of the database file. Exception: {0}", ex.Message);
            }
        }
    }
}
