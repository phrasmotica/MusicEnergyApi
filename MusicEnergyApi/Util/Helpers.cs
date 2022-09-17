using System;
using SpotifyAPI.Web;

namespace MusicEnergyApi
{
    /// <summary>
    /// Helper functions.
    /// </summary>
    public class Helpers
    {
        /// <summary>
        /// Returns an authenticated Spotify API client.
        /// </summary>
        public static SpotifyClient GetSpotifyClient()
        {
            var clientId = Environment.GetEnvironmentVariable("SpotifyClientId");
            var clientSecret = Environment.GetEnvironmentVariable("SpotifyClientSecret");

            var auth = new ClientCredentialsAuthenticator(clientId, clientSecret);

            var config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(auth);

            return new SpotifyClient(config);
        }
    }
}
