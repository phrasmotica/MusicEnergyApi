namespace MusicEnergyApi
{
    /// <summary>
    /// Model for a result returned from a track search.
    /// </summary>
    public class TrackSearchResult
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public int Year { get; set; }

        public string ArtworkUrl { get; set; }
    }
}
