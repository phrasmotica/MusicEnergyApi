using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;

namespace MusicEnergyCalculator
{
    public static class MusicEnergyCalculator
    {
        [FunctionName("MusicEnergyCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            string trackId = req.Query["track"];
            if (string.IsNullOrEmpty(trackId))
            {
                return new BadRequestObjectResult("Please supply a track ID!");
            }

            log.LogInformation($"Received track ID {trackId}");

            var track = await GetTrack(trackId);
            log.LogInformation($"Getting audio features for {track.Name} by {track.GetArtistName()}");

            var features = await GetAudioFeatures(trackId);
            if (features.HasError())
            {
                log.LogError(features.Error.Message);

                return features.Error.Status switch
                {
                    (int) HttpStatusCode.BadRequest => new NotFoundObjectResult($"Track '{trackId}' not found!"),
                    _ => new BadRequestObjectResult("An unknown error occurred."),
                };
            }

            var response = CreateTrackResponse(track, features);
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Returns a response for the given track and its audio features.
        /// </summary>
        private static TrackResponse CreateTrackResponse(FullTrack track, AudioFeatures features)
        {
            return new TrackResponse
            {
                Id = track.Id,
                Name = track.Name,
                Artist = track.GetArtistName(),
                Album = track.Album.Name,
                Year = track.GetReleaseYear(),
                ArtworkUrl = track.GetArtworkUrl(),

                Acousticness = features.Acousticness,
                Danceability = features.Danceability,
                Energy = features.Energy,
                Instrumentalness = features.Instrumentalness,
                Liveness = features.Liveness,
                NormalisedLoudness = features.NormalisedLoudness(),
                Speechiness = features.Speechiness,
                Valence = features.Valence,

                MondayEnergy = GetMondayEnergyScore(features),
                TuesdayEnergy = GetTuesdayEnergyScore(features),
                WednesdayEnergy = GetWednesdayEnergyScore(features),
                ThursdayEnergy = GetThursdayEnergyScore(features),
                FridayEnergy = GetFridayEnergyScore(features),
            };
        }

        /// <summary>
        /// Returns a Monday energy score for the given audio features.
        /// "experimental / noise / metal / grind / sixth world / misanthropic stuff"
        /// </summary>
        private static double GetMondayEnergyScore(AudioFeatures features)
        {
            return 0.3 * (1 - features.Valence) // misanthropic
                 + 0.3 * (1 - features.Energy) // dispondent
                 + 0.2 * features.NormalisedLoudness()
                 + 0.1 * features.Instrumentalness
                 + 0.1 * (1 - features.Danceability); // less danceable
        }

        /// <summary>
        /// Returns a Tuesday energy score for the given audio features.
        /// "techno / idm / glitch / illbient / deconstructed club / ambient / experimental"
        /// </summary>
        private static double GetTuesdayEnergyScore(AudioFeatures features)
        {
            return 0.4 * features.Instrumentalness
                 + 0.3 * features.NormalisedLoudness()
                 + 0.2 * features.Danceability
                 + 0.1 * (1 - features.Energy);
        }

        /// <summary>
        /// Returns a Wednesday energy score for the given audio features.
        /// "ethereal / confident / uplifting / new age / majestic / orchestral / psychedelic"
        /// </summary>
        private static double GetWednesdayEnergyScore(AudioFeatures features)
        {
            return 0.3 * features.NormalisedLoudness()
                 + 0.3 * features.Instrumentalness
                 + 0.2 * features.Acousticness
                 + 0.1 * features.Speechiness;
        }

        /// <summary>
        /// Returns a Thursday energy score for the given audio features.
        /// "joyous / confident / enigmatic / arrogant / charismatic"
        /// </summary>
        private static double GetThursdayEnergyScore(AudioFeatures features)
        {
            return 0.4 * features.Instrumentalness
                 + 0.3 * features.NormalisedLoudness()
                 + 0.2 * features.Danceability
                 + 0.1 * features.Energy;
        }

        /// <summary>
        /// Returns a Friday energy score for the given audio features.
        /// "rave / hedonistic / party / unhinged / unstoppable"
        /// </summary>
        private static double GetFridayEnergyScore(AudioFeatures features)
        {
            return 0.4 * features.Energy
                 + 0.25 * features.Danceability
                 + 0.25 * features.Valence
                 + 0.05 * features.NormalisedLoudness()
                 + 0.05 * features.Liveness;
        }

        /// <summary>
        /// Returns data for the track with the given ID.
        /// </summary>
        private static async Task<FullTrack> GetTrack(string trackId)
        {
            using var client = await GetClient();
            return await client.GetTrackAsync(trackId);
        }

        /// <summary>
        /// Returns audio feature data for the track with the given ID.
        /// </summary>
        private static async Task<AudioFeatures> GetAudioFeatures(string trackId)
        {
            using var client = await GetClient();
            return await client.GetAudioFeaturesAsync(trackId);
        }

        /// <summary>
        /// Returns an authenticated Spotify API client.
        /// </summary>
        private static async Task<SpotifyWebAPI> GetClient()
        {
            var clientId = Environment.GetEnvironmentVariable("SpotifyClientId");
            var clientSecret = Environment.GetEnvironmentVariable("SpotifyClientSecret");
            var auth = new CredentialsAuth(clientId, clientSecret);
            var token = await auth.GetToken();

            return new SpotifyWebAPI
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };
        }
    }

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
