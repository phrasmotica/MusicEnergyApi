using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace MusicEnergyCalculator
{
    /// <summary>
    /// Helper functions.
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Returns an authenticated Spotify API client.
        /// </summary>
        public static async Task<SpotifyWebAPI> GetSpotifyClient()
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
}
