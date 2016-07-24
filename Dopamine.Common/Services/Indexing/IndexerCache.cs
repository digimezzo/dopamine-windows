using Dopamine.Core.Database;
using Dopamine.Core.Database.Entities;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Common.Services.Indexing
{
    public class IndexerCache
    {
        #region Variables
        private DopamineContext context;
        private Dictionary<long, string> albumsDictionary;
        private Dictionary<long, string> artistsDictionary;
        private Dictionary<long, string> genresDictionary;
        private long maxAlbumID;
        private long maxArtistID;
        private long maxGenreID;
        #endregion

        #region Construction
        public IndexerCache(DopamineContext context)
        {
            this.context = context;

            this.Initialize();
        }
        #endregion

        #region "Public"
        public bool GetCachedArtist(ref Artist artist)
        {

            bool isCachedArtist = false;
            long similarArtistId = 0;

            Artist tempArtist = artist; // Because we cannot use ref parameters in a lambda expression

            try
            {
                similarArtistId = this.artistsDictionary.Where((a) => a.Value.Equals(this.GetArtistCacheComparer(tempArtist))).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem checking if Artist '{0}' exists in the cache. Exception: {1}", artist.ArtistName, ex.Message);
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
                this.artistsDictionary.Add(artist.ArtistID, this.GetArtistCacheComparer(artist));  // Keep the cache in sync with the context
            }

            return isCachedArtist;
        }

        public bool GetCachedGenre(ref Genre genre)
        {
            bool isCachedGenre = false;
            long similarGenreId = 0;

            Genre tempGenre = genre; // Because we cannot use ByRef parameters in a lambda expression

            try
            {
                similarGenreId = this.genresDictionary.Where((g) => g.Value.Equals(this.GetGenreCacheComparer(tempGenre))).Select((g) => g.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem checking if Genre '{0}' exists in the cache. Exception: {1}", genre.GenreName, ex.Message);
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
                this.genresDictionary.Add(genre.GenreID, this.GetGenreCacheComparer(genre)); // Keep the cache in sync with the context
            }

            return isCachedGenre;
        }

        public bool GetCachedAlbum(ref Album album)
        {
            bool isCachedAlbum = false;
            long similarAlbumId = 0;

            Album tempAlbum = album;

            try
            {
                similarAlbumId = this.albumsDictionary.Where((a) => a.Value.Equals(this.GetAlbumCacheComparer(tempAlbum))).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("There was a problem checking if Album '{0} / {1}' exists in the cache. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
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
                this.albumsDictionary.Add(album.AlbumID, this.GetAlbumCacheComparer(album)); // Keep the cache in sync with the context
            }

            return isCachedAlbum;
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Comparing new and existing object will happen in a Dictionary cache. This should improve performance.
            // For Albums, we're comparing a concatenated string consisting of AlbumTitle and AlbumArtist.
            this.albumsDictionary = this.context.Albums.ToDictionary(alb => alb.AlbumID, alb => this.GetAlbumCacheComparer(alb));
            this.artistsDictionary = this.context.Artists.ToDictionary(art => art.ArtistID, art => this.GetArtistCacheComparer(art));
            this.genresDictionary = this.context.Genres.ToDictionary(gen => gen.GenreID, gen => this.GetGenreCacheComparer(gen));

            this.maxAlbumID = this.albumsDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxArtistID = this.artistsDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxGenreID = this.genresDictionary.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
        }

        private string GetArtistCacheComparer(Artist iArtist)
        {
            return iArtist.ArtistNameTrim;
        }

        private string GetGenreCacheComparer(Genre iGenre)
        {
            return iGenre.GenreNameTrim;
        }

        private string GetAlbumCacheComparer(Album album)
        {
            // The guid makes sure the separator is unique
            return string.Format("{0}%69e91179-ad03-4646-a19b-46855f97ca91%{1}", album.AlbumTitleTrim, album.AlbumArtistTrim);
        }
        #endregion

    }
}
