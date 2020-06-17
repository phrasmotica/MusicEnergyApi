namespace MusicEnergyApi
{
    /// <summary>
    /// Model for a result returned from a track search.
    /// </summary>
    public class TrackSearchResult
    {
        /// <summary>
        /// Gets or sets the ID of the track.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the track.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the artist of the track.
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Gets or sets the album of the track.
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// Gets or sets the year of the track.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the URL of artwork for the track.
        /// </summary>
        public string ArtworkUrl { get; set; }
    }
}
