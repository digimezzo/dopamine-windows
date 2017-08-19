using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Logging;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Core.Services.Indexing
{
    internal class IndexerCache
    {
        #region Variables
        private HashSet<string> trackStatisticHashSet;
        private Dictionary<long, int> albumsDictionary;
        private Dictionary<long, int> artistsDictionary;
        private Dictionary<long, int> genresDictionary;

        private long maxAlbumID;
        private long maxArtistID;
        private long maxGenreID;

        private SQLiteConnectionFactory factory;
        #endregion

        #region Construction
        public IndexerCache()
        {
            this.factory = new SQLiteConnectionFactory();
            this.Initialize();
        }
        #endregion

        #region Public
        public bool HasCachedTrackStatistic(TrackStatistic trackStatistic)
        {
            if (trackStatisticHashSet.Contains(trackStatistic.SafePath))
            {
                return true;
            }

            trackStatisticHashSet.Add(trackStatistic.SafePath);

            return false;
        }

        public bool HasCachedArtist(ref Artist artist)
        {

            bool isCachedArtist = false;
            long similarArtistId = 0;

            Artist tempArtist = artist; // Because we cannot use ref parameters in a lambda expression

            try
            {
                similarArtistId = this.artistsDictionary.Where((a) => a.Value.Equals(tempArtist.GetHashCode())).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                CoreLogger.Error("There was a problem checking if Artist '{0}' exists in the cache. Exception: {1}", artist.ArtistName, ex.Message);
            }

            if (similarArtistId != 0)
            {
                isCachedArtist = true;
                artist.ArtistID = similarArtistId;
            }
            else
            {
                this.maxArtistID += 1;
                artist.ArtistID = this.maxArtistID;
                this.artistsDictionary.Add(artist.ArtistID, artist.GetHashCode());  // Keep the cache in sync with the context
            }

            return isCachedArtist;
        }

        public bool HasCachedGenre(ref Genre genre)
        {
            bool isCachedGenre = false;
            long similarGenreId = 0;

            Genre tempGenre = genre; // Because we cannot use ByRef parameters in a lambda expression

            try
            {
                similarGenreId = this.genresDictionary.Where((g) => g.Value.Equals(tempGenre.GetHashCode())).Select((g) => g.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                CoreLogger.Error("There was a problem checking if Genre '{0}' exists in the cache. Exception: {1}", genre.GenreName, ex.Message);
            }

            if (similarGenreId != 0)
            {
                isCachedGenre = true;
                genre.GenreID = similarGenreId;
            }
            else
            {
                this.maxGenreID += 1;
                genre.GenreID = this.maxGenreID;
                this.genresDictionary.Add(genre.GenreID, genre.GetHashCode()); // Keep the cache in sync with the context
            }

            return isCachedGenre;
        }

        public bool HasCachedAlbum(ref Album album)
        {
            bool isCachedAlbum = false;
            long similarAlbumId = 0;

            Album tempAlbum = album;

            try
            {
                similarAlbumId = this.albumsDictionary.Where((a) => a.Value.Equals(tempAlbum.GetHashCode())).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                CoreLogger.Error("There was a problem checking if Album '{0} / {1}' exists in the cache. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
            }

            if (similarAlbumId != 0)
            {
                isCachedAlbum = true;
                album.AlbumID = similarAlbumId;
            }
            else
            {
                this.maxAlbumID += 1;
                album.AlbumID = this.maxAlbumID;
                this.albumsDictionary.Add(album.AlbumID, album.GetHashCode()); // Keep the cache in sync with the context
            }

            return isCachedAlbum;
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Comparing new and existing object will happen in a Dictionary cache. This should improve performance.
            // For Albums, we're comparing a concatenated string consisting of AlbumTitle and AlbumArtist.
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                this.trackStatisticHashSet = new HashSet<string>(conn.Table<TrackStatistic>().ToList().Select(ts => ts.SafePath).ToList());
                this.albumsDictionary = conn.Table<Album>().ToDictionary(alb => alb.AlbumID, alb => alb.GetHashCode());
                this.artistsDictionary = conn.Table<Artist>().ToDictionary(art => art.ArtistID, art => art.GetHashCode());
                this.genresDictionary = conn.Table<Genre>().ToDictionary(gen => gen.GenreID, gen => gen.GetHashCode());
            }

            this.maxAlbumID = this.albumsDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxArtistID = this.artistsDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxGenreID = this.genresDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
        }
        #endregion
    }
}
