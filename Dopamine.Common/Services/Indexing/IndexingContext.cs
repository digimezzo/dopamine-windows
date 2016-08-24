using System.Collections.Generic;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using SQLite;

namespace Dopamine.Common.Services.Indexing
{
    public class IndexingContext
    {
        #region Variables
        private SQLiteConnectionFactory factory;

        private List<Track> newTracks;
        private List<Artist> newArtists;
        private List<Album> newAlbums;
        private List<Genre> newGenres;

        private List<Track> updatedTracks;
        private List<Album> updatedAlbums;
        #endregion

        #region Construction
        public IndexingContext()
        {
            this.factory = new SQLiteConnectionFactory();

            newTracks = new List<Track>();
            newArtists = new List<Artist>();
            newAlbums = new List<Album>();
            newGenres = new List<Genre>();

            updatedTracks = new List<Track>();
            updatedAlbums = new List<Album>();
        }
        #endregion

        #region Public
        public void Insert<T>(T t)
        {
            object obj = (object)t;

            if (t is Track)
            {
                this.newTracks.Add((Track)obj);
            }
            else if (t is Album)
            {
                this.newAlbums.Add((Album)obj);
            }
            else if (t is Artist)
            {
                this.newArtists.Add((Artist)obj);
            }
            else if (t is Genre)
            {
                this.newGenres.Add((Genre)obj);
            }
        }

        public void Update<T>(T t)
        {
            object obj = (object)t;

            if (t is Track)
            {
                this.updatedTracks.Add((Track)obj);
            }
            else if (t is Album)
            {
                this.updatedAlbums.Add((Album)obj);
            }
        }

        public void SaveChanges()
        {
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                // New Tracks
                conn.InsertAll(this.newTracks);
                this.newTracks.Clear();

                // Updated Tracks
                conn.UpdateAll(this.updatedTracks);
                this.updatedTracks.Clear();

                // New Artists
                conn.InsertAll(this.newArtists);
                this.newArtists.Clear();

                // New Albums
                conn.InsertAll(this.newAlbums);
                this.newAlbums.Clear();

                // Updated Albums
                conn.UpdateAll(this.updatedAlbums);
                this.updatedAlbums.Clear();

                // New Genres
                conn.InsertAll(this.newGenres);
                this.newGenres.Clear();
            }
        }
        #endregion
    }
}
