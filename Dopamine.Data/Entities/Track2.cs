using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("Tracks")]
    public class Track2
    {
        [Column("id"), PrimaryKey(), AutoIncrement()]
        public long Id { get; set; }

        [Column("name"), Unique(), Collation("NOCASE"), NotNull()]
        public string Name { get; set; }

        [Column("path"), Indexed()]
        public string Path { get; set; }

        [Column("folder_id"), Indexed(), NotNull()]
        public long FolderId { get; set; }

        [Column("filesize")]
        public long? Filesize { get; set; }

        [Column("bitrate")]
        public long? Bitrate { get; set; }

        [Column("samplerate")]
        public long? Samplerate { get; set; }

        [Column("duration")]
        public long? Duration { get; set; }

        [Column("year")]
        public long? Year { get; set; }

        [Column("language")]
        public string Language { get; set; }

        [Column("date_added")]
        public long DateAdded { get; set; }

        [Column("date_deleted")]
        public long? DateDeleted { get; set; }

        [Column("rating")]
        public long? Rating { get; set; }

        [Column("love")]
        public long? Love { get; set; }

        public static Track2 CreateDefault(string path)
        {
            var track = new Track2()
            {
                Path = path,
                DateAdded = DateTime.Now.Ticks
            };

            return track;
        }

        public Track2 ShallowCopy()
        {
            return (Track2)this.MemberwiseClone();
        }


        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return this.Path.Equals(((Track)obj).Path);
        }

        public override int GetHashCode()
        {
            return new { this.Path }.GetHashCode();
        }
    }
}
