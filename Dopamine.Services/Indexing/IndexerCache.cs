using Digimezzo.Utilities.Log;
using Dopamine.Data.Contracts;
using Dopamine.Data.Contracts.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dopamine.Services.Indexing
{
    internal class IndexerCache
    {
        private HashSet<string> cachedTrackStatistics;
        private Dictionary<long, Album> cachedAlbums;
        private Dictionary<long, Artist> cachedArtists;
        private Dictionary<long, Genre> cachedGenres;
        private Dictionary<long, Track> cachedTracks;

        private long maxAlbumID;
        private long maxArtistID;
        private long maxGenreID;

        private ISQLiteConnectionFactory factory;

        public IndexerCache(ISQLiteConnectionFactory factory)
        {
            this.factory = factory;
        }

        public bool HasCachedTrackStatistic(TrackStatistic trackStatistic)
        {
            if (this.cachedTrackStatistics.Contains(trackStatistic.SafePath))
            {
                return true;
            }

            this.cachedTrackStatistics.Add(trackStatistic.SafePath);

            return false;
        }

        public bool HasCachedArtist(ref Artist artist)
        {

            bool hasCachedArtist = false;
            long similarArtistId = 0;

            Artist tempArtist = artist; // Because we cannot use ref parameters in a lambda expression

            try
            {
                similarArtistId = this.cachedArtists.Where((a) => a.Value.Equals(tempArtist)).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Artist '{0}' exists in the cache. Exception: {1}", artist.ArtistName, ex.Message);
            }

            if (similarArtistId != 0)
            {
                hasCachedArtist = true;
                artist.ArtistID = similarArtistId;
            }
            else
            {
                this.maxArtistID += 1;
                artist.ArtistID = this.maxArtistID;
                this.cachedArtists.Add(artist.ArtistID, artist);  // Keep the cache in sync with the context
            }

            return hasCachedArtist;
        }

        public bool HasCachedGenre(ref Genre genre)
        {
            bool hasCachedGenre = false;
            long similarGenreId = 0;

            Genre tempGenre = genre; // Because we cannot use ByRef parameters in a lambda expression

            try
            {
                similarGenreId = this.cachedGenres.Where((g) => g.Value.Equals(tempGenre)).Select((g) => g.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Genre '{0}' exists in the cache. Exception: {1}", genre.GenreName, ex.Message);
            }

            if (similarGenreId != 0)
            {
                hasCachedGenre = true;
                genre.GenreID = similarGenreId;
            }
            else
            {
                this.maxGenreID += 1;
                genre.GenreID = this.maxGenreID;
                this.cachedGenres.Add(genre.GenreID, genre); // Keep the cache in sync with the context
            }

            return hasCachedGenre;
        }

        public bool HasCachedAlbum(ref Album album)
        {
            bool hasCachedAlbum = false;
            long similarAlbumId = 0;

            Album tempAlbum = album;

            try
            {
                similarAlbumId = this.cachedAlbums.Where((a) => a.Value.Equals(tempAlbum)).Select((a) => a.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Album '{0} / {1}' exists in the cache. Exception: {2}", album.AlbumTitle, album.AlbumArtist, ex.Message);
            }

            if (similarAlbumId != 0)
            {
                hasCachedAlbum = true;
                album.AlbumID = similarAlbumId;
            }
            else
            {
                this.maxAlbumID += 1;
                album.AlbumID = this.maxAlbumID;
                this.cachedAlbums.Add(album.AlbumID, album); // Keep the cache in sync with the context
            }

            return hasCachedAlbum;
        }

        public bool HasCachedTrack(ref Track track)
        {
            bool hasCachedTrack = false;
            long similarTrackId = 0;

            Track tempTrack = track;

            try
            {
                similarTrackId = this.cachedTracks.Where((t) => t.Value.Equals(tempTrack)).Select((t) => t.Key).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogClient.Error("There was a problem checking if Track with path '{0}' exists in the cache. Exception: {1}", track.Path, ex.Message);
            }

            if (similarTrackId != 0)
            {
                hasCachedTrack = true;
                track.TrackID = similarTrackId;
            }

            return hasCachedTrack;
        }

        public void AddTrack(Track track)
        {
            this.cachedTracks.Add(track.TrackID, track);
        }

        public void Initialize()
        {
            // Comparing new and existing object will happen in a Dictionary cache. This should improve performance.
            // For Albums, we're comparing a concatenated string consisting of AlbumTitle and AlbumArtist.
            using (SQLiteConnection conn = this.factory.GetConnection())
            {
                this.cachedTrackStatistics = new HashSet<string>(conn.Table<TrackStatistic>().ToList().Select(ts => ts.SafePath).ToList());
                this.cachedAlbums = conn.Table<Album>().ToDictionary(alb => alb.AlbumID, alb => alb);
                this.cachedArtists = conn.Table<Artist>().ToDictionary(art => art.ArtistID, art => art);
                this.cachedGenres = conn.Table<Genre>().ToDictionary(gen => gen.GenreID, gen => gen);
                this.cachedTracks = conn.Table<Track>().ToDictionary(trk => trk.TrackID, trk => trk);
            }

            this.maxAlbumID = this.cachedAlbums.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxArtistID = this.cachedArtists.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
            this.maxGenreID = this.cachedGenres.Keys.OrderByDescending(key => key).Select(key => key).FirstOrDefault();
        }
    }
}
