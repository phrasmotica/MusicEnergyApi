namespace MusicEnergyCalculator
{
    /// <summary>
    /// Model for responses.
    /// </summary>
    public class TrackResponse
    {
        /// <summary>
        /// Gets or sets the ID of the track.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the acousticness of the track.
        /// </summary>
        public float Acousticness { get; set; }

        /// <summary>
        /// Gets or sets the danceability of the track.
        /// </summary>
        public float Danceability { get; set; }

        /// <summary>
        /// Gets or sets the energy of the track.
        /// </summary>
        public float Energy { get; set; }

        /// <summary>
        /// Gets or sets the instrumentalness of the track.
        /// </summary>
        public float Instrumentalness { get; set; }

        /// <summary>
        /// Gets or sets the liveness of the track.
        /// </summary>
        public float Liveness { get; set; }

        /// <summary>
        /// Gets or sets the normalised loudness of the track.
        /// </summary>
        public float NormalisedLoudness { get; set; }

        /// <summary>
        /// Gets or sets the speechiness of the track.
        /// </summary>
        public float Speechiness { get; set; }

        /// <summary>
        /// Gets or sets the valence of the track.
        /// </summary>
        public float Valence { get; set; }

        /// <summary>
        /// Gets or sets the Monday energy score of the track.
        /// </summary>
        public double MondayEnergy { get; set; }

        /// <summary>
        /// Gets or sets the Tuesday energy score of the track.
        /// </summary>
        public double TuesdayEnergy { get; set; }

        /// <summary>
        /// Gets or sets the Wednesday energy score of the track.
        /// </summary>
        public double WednesdayEnergy { get; set; }

        /// <summary>
        /// Gets or sets the Thursday energy score of the track.
        /// </summary>
        public double ThursdayEnergy { get; set; }

        /// <summary>
        /// Gets or sets the Friday energy score of the track.
        /// </summary>
        public double FridayEnergy { get; set; }
    }
}
