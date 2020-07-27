using Dopamine.Core.Extensions;
using SQLite;
using System;

namespace Dopamine.Data.Entities
{
    [Table("TrackArtists")]
    public class TrackArtist
    {
        [Column("track_id"), Indexed(), NotNull()]
        public long TrackId { get; set; }

        [Column("artist_id"), Indexed(), NotNull()]
        public long ArtistId { get; set; }

        [Column("artist_role_id"), Indexed()]
        public long? ArtistRoleId { get; set; }

    }
}
