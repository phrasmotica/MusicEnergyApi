using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;

namespace MusicEnergyApi
{
    public static class MusicEnergyCalculator
    {
        [FunctionName("MusicEnergyCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            string trackId = req.Query["track"];
            if (string.IsNullOrWhiteSpace(trackId))
            {
                return new BadRequestObjectResult("Please supply a track ID!");
            }

            log.LogInformation($"Received track ID {trackId}");

            var (track, errorCode1) = await GetTrack(trackId, log);
            if (errorCode1 is not null)
            {
                return errorCode1 switch
                {
                    HttpStatusCode.BadRequest => new NotFoundObjectResult($"Track '{trackId}' not found!"),
                    _ => new BadRequestObjectResult("An unknown error occurred."),
                };
            }

            log.LogInformation($"Getting audio features for {track.Name} by {track.GetArtistName()}");

            var (features, errorCode2) = await GetAudioFeatures(trackId, log);
            if (errorCode2 is not null)
            {
                return errorCode2 switch
                {
                    HttpStatusCode.BadRequest => new NotFoundObjectResult($"Audio features for track '{trackId}' not found!"),
                    _ => new BadRequestObjectResult("An unknown error occurred."),
                };
            }

            var response = CreateEnergyResponse(track, features);
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Returns an energy response for the given track and its audio features.
        /// </summary>
        private static TrackEnergyResponse CreateEnergyResponse(FullTrack track, TrackAudioFeatures features)
        {
            return new TrackEnergyResponse
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

                Misanthropy = GetMisanthropyScore(features),
                Hypnotism = GetHypnotismScore(features),
                Majesty = GetMajestyScore(features),
                Confidence = GetConfidenceScore(features),
                Hedonism = GetHedonismScore(features),
            };
        }

        /// <summary>
        /// Returns a "misanthropy" score for the given audio features.
        /// "experimental / noise / metal / grind / sixth world / misanthropic stuff"
        /// </summary>
        private static double GetMisanthropyScore(TrackAudioFeatures features)
        {
            return 0.3 * (1 - features.Valence) // misanthropic
                 + 0.3 * (1 - features.Energy) // dispondent
                 + 0.2 * features.NormalisedLoudness()
                 + 0.1 * features.Instrumentalness
                 + 0.1 * (1 - features.Danceability); // less danceable
        }

        /// <summary>
        /// Returns a "hypnotism" score for the given audio features.
        /// "techno / idm / glitch / illbient / deconstructed club / ambient / experimental"
        /// </summary>
        private static double GetHypnotismScore(TrackAudioFeatures features)
        {
            return 0.4 * features.Instrumentalness
                 + 0.3 * features.NormalisedLoudness()
                 + 0.2 * features.Danceability
                 + 0.1 * (1 - features.Energy);
        }

        /// <summary>
        /// Returns a "majesty" score for the given audio features.
        /// "ethereal / confident / uplifting / new age / majestic / orchestral / psychedelic"
        /// </summary>
        private static double GetMajestyScore(TrackAudioFeatures features)
        {
            return 0.3 * features.NormalisedLoudness()
                 + 0.3 * features.Instrumentalness
                 + 0.2 * features.Acousticness
                 + 0.1 * features.Speechiness;
        }

        /// <summary>
        /// Returns a "confidence" score for the given audio features.
        /// "joyous / confident / enigmatic / arrogant / charismatic"
        /// </summary>
        private static double GetConfidenceScore(TrackAudioFeatures features)
        {
            return 0.4 * features.Instrumentalness
                 + 0.3 * features.NormalisedLoudness()
                 + 0.2 * features.Danceability
                 + 0.1 * features.Energy;
        }

        /// <summary>
        /// Returns a "hedonism" score for the given audio features.
        /// "rave / hedonistic / party / unhinged / unstoppable"
        /// </summary>
        private static double GetHedonismScore(TrackAudioFeatures features)
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
        private static async Task<(FullTrack, HttpStatusCode?)> GetTrack(string trackId, ILogger log)
        {
            var client = Helpers.GetSpotifyClient();

            try
            {
                var response = await client.Tracks.Get(trackId);
                return (response, null);
            }
            catch (APIException e)
            {
                log.LogError($"[{nameof(GetTrack)}] error getting track with ID '{trackId}': {e.Message}");

                return (null, e.Response.StatusCode);
            }
        }

        /// <summary>
        /// Returns audio feature data for the track with the given ID.
        /// </summary>
        private static async Task<(TrackAudioFeatures, HttpStatusCode?)> GetAudioFeatures(string trackId, ILogger log)
        {
            var client = Helpers.GetSpotifyClient();

            try
            {
                var response = await client.Tracks.GetAudioFeatures(trackId);
                return (response, null);
            }
            catch (APIException e)
            {
                log.LogError($"[{nameof(GetAudioFeatures)}] error getting audio features for track with ID '{trackId}': {e.Message}");

                return (null, e.Response.StatusCode);
            }
        }
    }
}
