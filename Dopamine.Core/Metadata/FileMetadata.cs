using Dopamine.Core.Base;
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
        #endregion

        #region Construction
        public FileMetadata(string filePath)
        {
            ByteVector.UseBrokenLatin1Behavior = true; // Otherwise Latin1 is used as default, which causes characters in various languages being displayed wrong.
            this.file = TagLib.File.Create(filePath);
        }
        #endregion

        #region ReadOnly Properties
        public string FileName
        {
            get { return this.file.Name; }
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
                if (this.title == null) this.title = new MetadataValue { Value = this.file.Tag.Title };
                return this.title;
            }
            set
            {
                this.title = value;
                if (value.IsValueChanged) this.file.Tag.Title = value.Value;
            }
        }

        public MetadataValue Album
        {
            get
            {
                if (this.album == null) this.album = new MetadataValue { Value = this.file.Tag.Album };
                return this.album;
            }
            set
            {
                this.album = value;
                if (value.IsValueChanged) this.file.Tag.Album = value.Value;
            }
        }

        public MetadataValue AlbumArtists
        {
            get
            {
                if (this.albumArtists == null) this.albumArtists = new MetadataValue { Values = this.file.Tag.AlbumArtists };
                return this.albumArtists;
            }
            set
            {
                this.albumArtists = value;
                if (value.IsValueChanged) this.file.Tag.AlbumArtists = value.Values;
            }
        }

        public MetadataValue Artists
        {
            get
            {
                if (this.artists == null) this.artists = new MetadataValue { Values = this.file.Tag.Performers };
                return this.artists;
            }
            set
            {
                this.artists = value;
                if (value.IsValueChanged) this.file.Tag.Performers = value.Values;
            }
        }

        public MetadataValue Genres
        {
            get
            {
                if (this.genres == null)
                    this.genres = new MetadataValue { Values = this.file.Tag.Genres };
                return this.genres;
            }
            set
            {
                this.genres = value;
                if (value.IsValueChanged) this.file.Tag.Genres = value.Values;
            }
        }

        public MetadataValue Comment
        {
            get
            {
                if (this.comment == null) this.comment = new MetadataValue { Value = this.file.Tag.Comment };
                return this.comment;
            }
            set
            {
                this.comment = value;
                if (value.IsValueChanged) this.file.Tag.Comment = value.Value;
            }
        }

        public MetadataValue Grouping
        {
            get
            {
                if (this.grouping == null) this.grouping = new MetadataValue { Value = this.file.Tag.Grouping };
                return new MetadataValue { Value = this.file.Tag.Grouping };
            }
            set
            {
                this.grouping = value;
                if (value.IsValueChanged) this.file.Tag.Grouping = value.Value;
            }
        }

        public MetadataValue Year
        {
            get
            {
                if (this.year == null) this.year = new MetadataValue { Value = this.file.Tag.Year == 0 ? string.Empty : this.file.Tag.Year.ToString() };
                return this.year;
            }
            set
            {
                this.year = value;
                if (value.IsValueChanged) this.file.Tag.Year = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue TrackNumber
        {
            get
            {
                if (this.trackNumber == null) this.trackNumber = new MetadataValue { Value = this.file.Tag.Track == 0 ? string.Empty : this.file.Tag.Track.ToString() };
                return this.trackNumber;
            }
            set
            {
                this.trackNumber = value;
                if (value.IsValueChanged) this.file.Tag.Track = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue TrackCount
        {
            get
            {
                if (this.trackCount == null)
                    this.trackCount = new MetadataValue { Value = this.file.Tag.TrackCount == 0 ? string.Empty : this.file.Tag.TrackCount.ToString() };
                return this.trackCount;
            }
            set
            {
                this.trackCount = value;
                if (value.IsValueChanged)
                    this.file.Tag.TrackCount = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue DiscNumber
        {
            get
            {
                if (this.discNumber == null)
                    this.discNumber = new MetadataValue { Value = this.file.Tag.Disc == 0 ? string.Empty : this.file.Tag.Disc.ToString() };
                return this.discNumber;
            }
            set
            {
                this.discNumber = value;
                if (value.IsValueChanged)
                    this.file.Tag.Disc = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataValue DiscCount
        {
            get
            {
                if (this.discCount == null)
                    this.discCount = new MetadataValue { Value = this.file.Tag.DiscCount == 0 ? string.Empty : this.file.Tag.DiscCount.ToString() };
                return this.discCount;
            }
            set
            {
                this.discCount = value;
                if (value.IsValueChanged)
                    this.file.Tag.DiscCount = string.IsNullOrEmpty(value.Value) ? (UInt32)0 : Convert.ToUInt32(value.Value);
            }
        }

        public MetadataRatingValue Rating
        {
            get
            {
                TagLib.Tag tag = this.file.GetTag(TagLib.TagTypes.Id3v2);

                if (tag != null)
                {
                    TagLib.Id3v2.PopularimeterFrame popM = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, Defaults.PopMUser, true);
                    if (this.rating == null)
                        this.rating = new MetadataRatingValue { Value = MetadataUtils.PopM2StarRating(popM.Rating) };
                }
                else
                {
                    this.rating = new MetadataRatingValue { Value = 0 };
                }

                return this.rating;
            }
            set
            {
                this.rating = value;

                if (value.IsValueChanged)
                {
                    TagLib.Tag tag = this.file.GetTag(TagLib.TagTypes.Id3v2);

                    if (tag != null)
                    {
                        TagLib.Id3v2.PopularimeterFrame PopM = TagLib.Id3v2.PopularimeterFrame.Get((TagLib.Id3v2.Tag)tag, Defaults.PopMUser, true);
                        PopM.Rating = MetadataUtils.Star2PopMRating(value.Value);
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
                    this.artworkData = new MetadataArtworkValue();
                    this.artworkData.SetValue(string.Empty, this.file.Tag.Pictures.Length >= 1 ? (byte[])this.file.Tag.Pictures[0].Data.Data : null);
                }

                return this.artworkData;
            }
            set
            {
                this.artworkData = value;


                if (value.IsValueChanged)
                {
                    if (value.DataValue == null)
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
                        pic.Data = value.DataValue;

                        this.file.Tag.Pictures = new Picture[1] { pic };
                    }
                }
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
    }
}
