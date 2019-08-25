using System.Linq;
using Nancy;
using NzbDrone.Core.Music;
using Lidarr.Http.Extensions;
using Nancy.Configuration;

namespace Lidarr.Api.V1.AlbumStudio
{
    public class AlbumStudioModule : LidarrV1Module
    {
        private readonly IArtistService _artistService;
        private readonly IAlbumMonitoredService _albumMonitoredService;

        public AlbumStudioModule(INancyEnvironment environment,
                                 IArtistService artistService,
                                 IAlbumMonitoredService albumMonitoredService)
        : base(environment, "/albumstudio")
        {
            _artistService = artistService;
            _albumMonitoredService = albumMonitoredService;
            Post("/",  artist => UpdateAll());
        }

        private Response UpdateAll()
        {
            //Read from request
            var request = Request.Body.FromJson<AlbumStudioResource>();
            var artistToUpdate = _artistService.GetArtists(request.Artist.Select(s => s.Id));

            foreach (var s in request.Artist)
            {
                var artist = artistToUpdate.Single(c => c.Id == s.Id);

                if (s.Monitored.HasValue)
                {
                    artist.Monitored = s.Monitored.Value;
                }

                if (request.MonitoringOptions != null && request.MonitoringOptions.Monitor == MonitorTypes.None)
                {
                    artist.Monitored = false;
                }

                _albumMonitoredService.SetAlbumMonitoredStatus(artist, request.MonitoringOptions);
            }

            return "ok".AsResponse(_environment, HttpStatusCode.Accepted);
        }
    }
}
