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
        /// <summary>
        /// The app client ID.
        /// </summary>
        private const string ClientId = "de8e2df8cbb84989bb42d13700e83168";

        /// <summary>
        /// The app client secret.
        /// </summary>
        private const string ClientSecret = "b6ff30e7e1744da9ac435eda959fbbf4";

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

            log.LogInformation($"Getting audio features for track '{trackId}'");

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

            var response = GetTrackResponse(features);
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Returns a track response for the given audio features.
        /// </summary>
        private static TrackResponse GetTrackResponse(AudioFeatures features)
        {
            return new TrackResponse
            {
                Id = features.Id,

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
            var auth = new CredentialsAuth(ClientId, ClientSecret);
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
        /// Returns the loudness from the given features normalised to a value between 0 and 1.
        /// </summary>
        public static float NormalisedLoudness(this AudioFeatures features)
        {
            return (features.Loudness / 60) + 1;
        }
    }
}
