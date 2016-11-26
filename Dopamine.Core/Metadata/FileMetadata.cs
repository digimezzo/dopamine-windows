using Dopamine.Core.Base;
using Dopamine.Core.Extensions;
using System;
using System.Linq;
using TagLib;

namespace Dopamine.Core.Metadata
{
    public class FileMetadata
    {
        #region Variables
        private TagLib.File file;
        private MetadataValue title;
        private MetadataValue album;
        private MetadataValue albumArtists;
        private MetadataValue artists;
        private MetadataValue genres;
        private MetadataValue comment;
        private MetadataValue grouping;
        private MetadataValue year;
        private MetadataValue trackNumber;
        private MetadataValue trackCount;
        private MetadataValue discNumber;
        private MetadataValue discCount;
        private MetadataRatingValue rating;
        private MetadataArtworkValue artworkData;
        private MetadataValue lyrics;
        #endregion

        #region Construction
        public FileMetadata(string filePath)
        {
            ByteVector.UseBrokenLatin1Behavior = true; // Otherwise Latin1 is used as default, which causes characters in various languages being displayed wrong.
            this.file = TagLib.File.Create(filePath);
        }
        #endregion

        #region ReadOnly Properties
        public string Path
        {
            get { return this.file.Name; }
        }

        public string SafePath
        {
            get { return this.file.Name.ToSafePath(); }
        }

        public int BitRate
        {
            get
            {
                // Workaround for a bug in taglibsharp. The Duration field  
                // must be set before the correct AudioBitrate is returned.
                TimeSpan dummy = this.file.Properties.Duration;
                return this.file.Properties.AudioBitrate;
            }
        }

        public int SampleRate
        {
            get { return this.file.Properties.AudioSampleRate; }
        }

        public TimeSpan Duration
        {
            get { return this.file.Properties.Duration; }
        }

        public string Type
        {
            get { return !string.IsNullOrEmpty(this.file.MimeType) && this.file.MimeType.Split('/').Count() > 1 ? this.file.MimeType.Split('/')[1].ToUpper() : string.Empty; }
        }

        public string MimeType
        {
            get { return this.file.MimeType; }
        }
        #endregion

        #region Properties
        public MetadataValue Title
        {
            get
            {
                if (this.title == null) this.title = new MetadataValue(this.file.Tag.Title == null ? string.Empty : this.file.Tag.Title);
                return this.title;
            }
            set
            {
                this.title = value;
                this.file.Tag.Title = value.Value;
            }
        }

        public MetadataValue Album
        {
            get
            {
                if (this.album == null) this.album = new MetadataValue(this.file.Tag.Album == null ? string.Empty : this.file.Tag.Album);
                return this.album;
            }
            set
            {
                this.album = value;
                this.file.Tag.Album = value.Value;
            }
        }

        public MetadataValue AlbumArtists
        {
            get
            {
                if (this.albumArtists == null) this.albumArtists = this.file.Tag.AlbumArtists == null ? new MetadataValue(string.Empty) : new MetadataValue(this.file.Tag.AlbumArtists);
                return this.albumArtists;
            }
            set
            {
                this.albumArtists = value;
                this.file.Tag.AlbumArtists = value.Values;
            }
        }

        public MetadataValue Artists
        {
            get
            {
                if (this.artists == null) this.artists = this.file.Tag.Performers == null ? new MetadataValue(string.Empty) : new MetadataValue(this.file.Tag.Performers);
                return this.artists;
            }
            set
            {
                this.artists = value;
                this.file.Tag.Performers = value.Values;
            }
        }

        public MetadataValue Genres
        {
            get
            {
                if (this.genres == null) this.genres = this.file.Tag.Genres == null ? new MetadataValue(string.Empty) : new MetadataValue(this.file.Tag.Genres);
                return this.genres;
            }
            set
            {
                this.genres = value;
                this.file.Tag.Genres = value.Values;
            }
        }

        public MetadataValue Comment
        {
            get
            {
                if (this.comment == null) this.comment = new MetadataValue(this.file.Tag.Comment == null ? string.Empty : this.file.Tag.Comment);
                return this.comment;
            }
            set
            {
                this.comment = value;
                this.file.Tag.Comment = value.Value;
            }
        }

        public MetadataValue Grouping
        {
            get
            {
                if (this.grouping == null) this.grouping = new MetadataValue(this.file.Tag.Grouping == null ? string.Empty : this.file.Tag.Grouping);
                return this.grouping;
            }
            set
            {
                this.grouping = value;
                this.file.Tag.Grouping = value.Value;
            }
        }

        public MetadataValue Year
        {
            get
            {
                if (this.year == null) this.year = new MetadataValue(this.file.Tag.Year == 0 ? string.Empty : this.file.Tag.Year.ToString());
                return this.year;
            }
            set
            {
                this.year = value;
                this.file.Tag.Year = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue TrackNumber
        {
            get
            {
                if (this.trackNumber == null) this.trackNumber = new MetadataValue(this.file.Tag.Track == 0 ? string.Empty : this.file.Tag.Track.ToString());
                return this.trackNumber;
            }
            set
            {
                this.trackNumber = value;
                this.file.Tag.Track = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue TrackCount
        {
            get
            {
                if (this.trackCount == null) this.trackCount = new MetadataValue(this.file.Tag.TrackCount == 0 ? string.Empty : this.file.Tag.TrackCount.ToString());
                return this.trackCount;
            }
            set
            {
                this.trackCount = value;
                this.file.Tag.TrackCount = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue DiscNumber
        {
            get
            {
                if (this.discNumber == null) this.discNumber = new MetadataValue(this.file.Tag.Disc == 0 ? string.Empty : this.file.Tag.Disc.ToString());
                return this.discNumber;
            }
            set
            {
                this.discNumber = value;
                this.file.Tag.Disc = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue DiscCount
        {
            get
            {
                if (this.discCount == null) this.discCount = new MetadataValue(this.file.Tag.DiscCount == 0 ? string.Empty : this.file.Tag.DiscCount.ToString());
                return this.discCount;
            }
            set
            {
                this.discCount = value;
                this.file.Tag.DiscCount = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataRatingValue Rating
        {
            get
            {
                if (System.IO.Path.GetExtension(this.file.Name.ToLower()) == FileFormats.MP3)
                {
                    Tag tag = this.file.GetTag(TagTypes.Id3v2);

                    if (tag != null & this.rating == null)
                    {
                        TagLib.Id3v2.PopularimeterFrame popMFrame;

                        // First, try to get the rating from the default Windows PopM user.
                        popMFrame = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, Defaults.WindowsPopMUser, true);

                        if (popMFrame != null && popMFrame.Rating > 0)
                        {
                            this.rating = new MetadataRatingValue(MetadataUtils.PopM2StarRating(popMFrame.Rating));
                        }
                        else
                        {
                            // No rating found for the default Windows PopM user. Try for other PopM users.
                            foreach (var user in Defaults.OtherPopMUsers)
                            {
                                popMFrame = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, user, true);

                                if (popMFrame != null && popMFrame.Rating > 0)
                                {
                                    this.rating = new MetadataRatingValue(MetadataUtils.PopM2StarRating(popMFrame.Rating));
                                    break; // As soon as we found a rating, stop.
                                }
                            }
                        }
                    }
                }

                if (this.rating == null) this.rating = new MetadataRatingValue();

                return this.rating;
            }
            set
            {
                this.rating = value;

                if (System.IO.Path.GetExtension(this.file.Name.ToLower()) == FileFormats.MP3)
                {
                    Tag tag = this.file.GetTag(TagTypes.Id3v2);

                    if (tag != null)
                    {
                        TagLib.Id3v2.PopularimeterFrame popMFrame = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, Defaults.WindowsPopMUser, true);
                        popMFrame.Rating = MetadataUtils.Star2PopMRating(value.Value);
                    }
                }
            }
        }

        public MetadataArtworkValue ArtworkData
        {
            get
            {
                if (this.artworkData == null)
                {
                    this.artworkData = new MetadataArtworkValue(this.file.Tag.Pictures.Length >= 1 ? (byte[])this.file.Tag.Pictures[0].Data.Data : null);
                }

                return this.artworkData;
            }
            set
            {
                this.artworkData = value;

                if (value.Value == null)
                {
                    // Remove all pictures
                    this.file.Tag.Pictures = new Picture[] { };
                }
                else
                {
                    var pic = new Picture();
                    pic.Type = PictureType.Other;
                    pic.MimeType = "image/jpeg";
                    pic.Description = "Cover";
                    pic.Data = value.Value;

                    this.file.Tag.Pictures = new Picture[1] { pic };
                }
            }
        }

        public MetadataValue Lyrics
        {
            get
            {
                if (this.lyrics == null) this.lyrics = new MetadataValue(this.file.Tag.Lyrics);
                return this.lyrics;
            }
            set
            {
                this.lyrics = value;
                this.file.Tag.Lyrics = value.Value;
            }
        }
        #endregion

        #region Public
        public void Save()
        {
            try
            {
                this.file.Save();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.SafePath.Equals(((FileMetadata)obj).SafePath);
        }

        public override int GetHashCode()
        {
            return new { this.SafePath }.GetHashCode();
        }
        #endregion
    }
}
