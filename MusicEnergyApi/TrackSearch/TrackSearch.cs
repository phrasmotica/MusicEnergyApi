using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web.Models;
using SpotifyAPI.Web.Enums;
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
            if (string.IsNullOrEmpty(query))
            {
                return new BadRequestObjectResult("Please supply a search query!");
            }

            log.LogInformation($"Received search query {query}");

            var searchItem = await SearchTrack(query);
            if (searchItem.HasError())
            {
                log.LogError(searchItem.Error.Message);

                return searchItem.Error.Status switch
                {
                    (int) HttpStatusCode.BadRequest => new NotFoundObjectResult($"Invalid search query '{query}'!"),
                    _ => new BadRequestObjectResult("An unknown error occurred."),
                };
            }

            var trackResults = searchItem.Tracks.Items.Select(CreateTrackSearchResult);
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
        private static async Task<SearchItem> SearchTrack(string query)
        {
            using var client = await Helpers.GetSpotifyClient();
            return await client.SearchItemsAsync(query, SearchType.Track, 10);
        }
    }
}
