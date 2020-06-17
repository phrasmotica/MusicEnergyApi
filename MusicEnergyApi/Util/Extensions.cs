using System;
using System.Linq;
using SpotifyAPI.Web.Models;

namespace MusicEnergyApi
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the names of the artists for the given track.
        /// </summary>
        public static string GetArtistName(this FullTrack track)
        {
            return string.Join(" / ", track.Artists.Select(a => a.Name));
        }

        /// <summary>
        /// Returns the release year of the given track.
        /// </summary>
        public static int GetReleaseYear(this FullTrack track)
        {
            if (track.Album.ReleaseDatePrecision.Equals("year", StringComparison.OrdinalIgnoreCase))
            {
                return int.Parse(track.Album.ReleaseDate);
            }

            var dateStr = track.Album.ReleaseDate;
            if (track.Album.ReleaseDatePrecision.Equals("month", StringComparison.OrdinalIgnoreCase))
            {
                dateStr += "-01";
            }

            return DateTime.Parse(dateStr).Year;
        }

        /// <summary>
        /// Returns the URL of artwork for the given track.
        /// </summary>
        public static string GetArtworkUrl(this FullTrack track)
        {
            return track.Album.Images.Aggregate((i1, i2) => i1.GetSize() > i2.GetSize() ? i1 : i2).Url;
        }

        /// <summary>
        /// Returns the size of the image.
        /// </summary>
        public static int GetSize(this Image image)
        {
            return image.Height * image.Width;
        }

        /// <summary>
        /// Returns the loudness from the given features normalised to a value between 0 and 1.
        /// </summary>
        public static float NormalisedLoudness(this AudioFeatures features)
        {
            return (features.Loudness / 60) + 1;
        }
    }
}
