namespace MusicEnergyApi
{
    /// <summary>
    /// Model for responses outlining track energy scores.
    /// </summary>
    public class TrackEnergyResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public int Year { get; set; }

        public string ArtworkUrl { get; set; }

        public float Acousticness { get; set; }

        public float Danceability { get; set; }

        public float Energy { get; set; }

        public float Instrumentalness { get; set; }

        public float Liveness { get; set; }

        public float NormalisedLoudness { get; set; }

        public float Speechiness { get; set; }

        public float Valence { get; set; }

        public double Misanthropy { get; set; }

        public double Hypnotism { get; set; }

        public double Majesty { get; set; }

        public double Confidence { get; set; }

        public double Hedonism { get; set; }
    }
}
