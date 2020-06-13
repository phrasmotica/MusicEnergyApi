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
        private const string ClientId = "de8e2df8cbb84989bb42d13700e83168";
        private const string ClientSecret = "b6ff30e7e1744da9ac435eda959fbbf4";

        [FunctionName("MusicEnergyCalculator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            string trackParam = req.Query["track"];
            var trackId = string.IsNullOrEmpty(trackParam) ? "31zBvESt331Jm2pU1R9tXt" : trackParam;
            log.LogInformation($"Getting audio features for track: {trackId}");

            var features = await GetAudioFeatures(trackId);
            var summary = GetSummary(features);
            return new OkObjectResult(summary);
        }

        /// <summary>
        /// Returns audio feature data for the track with the given ID.
        /// </summary>
        private static string GetSummary(AudioFeatures features)
        {
            return $"Track ID: {features.Id}\n"
                 + $"Acousticness: {features.Acousticness}\n"
                 + $"Danceability: {features.Danceability}\n"
                 + $"Energy: {features.Energy}\n"
                 + $"Instrumentalness: {features.Instrumentalness}\n"
                 + $"Liveness: {features.Liveness}\n"
                 + $"Loudness: {features.Loudness}\n"
                 + $"Speechiness: {features.Speechiness}\n"
                 + $"Valence: {features.Valence}\n";
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
}
