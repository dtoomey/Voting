using System.Collections.Generic;
using System.Web.Http;

namespace VotingService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Client;
    using VotingDataService;
    using System.Fabric;

    public class VotesController : ApiController
    {
        private const string REMOTING_URI = "fabric:/Voting/VotingDataService";
        private static IVotingDataService client = null;

        // Used for health checks.
        public static long _requestCount = 0L;

        // GET api/votes 
        [HttpGet]
        [Route("api/votes")]
        public async Task<HttpResponseMessage>  Get()
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Get", activityId);

            Interlocked.Increment(ref _requestCount);

            try
            {
                IVotingDataService client = GetRemotingClient();

                var votes = await client.GetAllVoteCounts();

                var response = Request.CreateResponse(HttpStatusCode.OK, votes);
                response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true, MustRevalidate = true };

                ServiceEventSource.Current.ServiceRequestStop("VotesController.Get", activityId);
                return response;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("Error in VotesController.Get method: {0}", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError,  "An error occurred: " + ex.Message);
            }
        }

        // GET api/totalVotes 
        [HttpGet]
        [Route("api/totalVotes")]
        public async Task<HttpResponseMessage> GetTotalVotes()
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.GetTotalVotes", activityId);

            Interlocked.Increment(ref _requestCount);

            try
            {
                IVotingDataService client = GetRemotingClient();

                var totalVotes = await client.GetTotalNumberOfVotes();

                var response = Request.CreateResponse(HttpStatusCode.OK, totalVotes);
                response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true, MustRevalidate = true };

                ServiceEventSource.Current.ServiceRequestStop("VotesController.GetTotalVotes", activityId);
                return response;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("Error in VotesController.GetTotalVotes method: {0}", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }

            var applicationName = new Uri("fabric:/Voting");
            using (var client = new FabricClient())
            {
                var applications = await client.QueryManager.GetApplicationListAsync(applicationName).ConfigureAwait(false);
                var version = applications[0].ApplicationTypeVersion;
            }
        }

        // GET api/appVersion 
        [HttpGet]
        [Route("api/appVersion")]
        public async Task<HttpResponseMessage> GetAppVersion()
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.GetAppVersion", activityId);

            Interlocked.Increment(ref _requestCount);

            string version;

            try
            {
                var applicationName = new Uri("fabric:/Voting");
                using (var client = new FabricClient())
                {
                    var applications = await client.QueryManager.GetApplicationListAsync(applicationName).ConfigureAwait(false);
                    version = applications[0].ApplicationTypeVersion;
                }

                var response = Request.CreateResponse(HttpStatusCode.OK, version);
                response.Headers.CacheControl = new CacheControlHeaderValue() { NoCache = true, MustRevalidate = true };

                ServiceEventSource.Current.ServiceRequestStop("VotesController.GetAppVersion", activityId);
                return response;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("Error in VotesController.GetAppVersion method: {0}", ex.Message);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "An error occurred: " + ex.Message);
            }

        }

        [HttpPost]
        [Route("api/{key}")]
        public async Task<HttpResponseMessage> Post(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Post", activityId);

            Interlocked.Increment(ref _requestCount);

            IVotingDataService client = GetRemotingClient();
            await client.AddVote(key);

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Post", activityId);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpDelete]
        [Route("api/{key}")]
        public async Task<HttpResponseMessage> Delete(string key)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.Delete", activityId);

            Interlocked.Increment(ref _requestCount);

            IVotingDataService client = GetRemotingClient();
            await client.DeleteVoteItem(key);

            ServiceEventSource.Current.ServiceRequestStop("VotesController.Delete", activityId);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("api/{file}")]
        public HttpResponseMessage GetFile(string file)
        {
            string activityId = Guid.NewGuid().ToString();
            ServiceEventSource.Current.ServiceRequestStart("VotesController.GetFile", activityId);

            string response = null;
            string responseType = "text/html";

            Interlocked.Increment(ref _requestCount);

            // Validate file name.
            if ("index.html" == file)
            {
                string path = string.Format(@"..\VotingServicePkg.Code.1.0.0\{0}", file);
                response = File.ReadAllText(path);
            }

            if (null != response)
            {
                ServiceEventSource.Current.ServiceRequestStop("VotesController.GetFile", activityId);
                return Request.CreateResponse(HttpStatusCode.OK, response, responseType);
            }
            else
            {
                ServiceEventSource.Current.ServiceRequestFailed("VotesController.GetFile", activityId, string.Format("Requested file '{0}' not found.", file));
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File");
            }
        }

        private static IVotingDataService GetRemotingClient()
        {
            if (client == null)
            {
                var resolver = ServicePartitionResolver.GetDefault();
                var partKey = new ServicePartitionKey(1);
                var partition = resolver.ResolveAsync(new Uri(REMOTING_URI), partKey, new CancellationToken());
                client = ServiceProxy.Create<IVotingDataService>(new Uri(REMOTING_URI), partKey);
            }
            return client;
        }


    }
}
