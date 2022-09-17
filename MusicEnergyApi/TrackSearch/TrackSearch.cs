using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using System.Net;
using System.Linq;

namespace MusicEnergyApi
{
    public static class TrackSearch
    {
        [FunctionName("TrackSearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            ILogger log)
        {
            string query = req.Query["query"];

            if (string.IsNullOrWhiteSpace(query))
            {
                return new BadRequestObjectResult("Please supply a search query!");
            }

            log.LogInformation($"Received search query {query}");

            var (response, errorCode) = await SearchTrack(query, log);
            if (errorCode is not null)
            {
                return errorCode switch
                {
                    HttpStatusCode.BadRequest => new NotFoundObjectResult($"Invalid search query '{query}'!"),
                    _ => new BadRequestObjectResult("An unknown error occurred."),
                };
            }

            var trackResults = response.Tracks.Items.Select(CreateTrackSearchResult);
            return new OkObjectResult(trackResults);
        }

        /// <summary>
        /// Returns a result model for the given track.
        /// </summary>
        private static TrackSearchResult CreateTrackSearchResult(FullTrack track)
        {
            return new TrackSearchResult
            {
                Id = track.Id,
                Name = track.Name,
                Artist = track.GetArtistName(),
                Album = track.Album.Name,
                Year = track.GetReleaseYear(),
                ArtworkUrl = track.GetArtworkUrl()
            };
        }

        /// <summary>
        /// Returns track results for the given search query.
        /// </summary>
        private static async Task<(SearchResponse, HttpStatusCode?)> SearchTrack(string query, ILogger log)
        {
            var client = Helpers.GetSpotifyClient();
            
            try
            {
                var request = new SearchRequest(SearchRequest.Types.Track, query)
                {
                    Limit = 10,
                };

                var response = await client.Search.Item(request);
                return (response, null);
            }
            catch (APIException e)
            {
                log.LogError($"[{nameof(SearchTrack)}] error executing track search query '{query}': {e.Message}");

                return (null, e.Response.StatusCode);
            }
        }
    }
}
