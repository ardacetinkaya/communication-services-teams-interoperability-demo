using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ACS_Teams_Interop
{
    public class GraphService
    {
        private readonly ILogger<GraphService> _logger;
        private IGraphAuthenticator _graphAuthenticator;

        public GraphService(ILogger<GraphService> logger)
        {
            _logger = logger;
        }

        public void Authenticate(IGraphAuthenticator authenticator)
        {
            this._graphAuthenticator = authenticator;
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(string method, string url, object content, string token = "", string contentType = "application/json")
        {
            if (_graphAuthenticator == null) throw new ArgumentNullException(nameof(_graphAuthenticator));
            try
            {
                var client = _graphAuthenticator.GetAuthenticatedClient(AuthenticatorProvider.ConfidentialUserClient, token);
                var requestUrl = $"{client.BaseUrl.Replace("/v1.0", "")}/{url}";
                _logger.LogDebug($"Request:{method} {requestUrl}");
                var request = new BaseRequest(requestUrl, client, null)
                {
                    Method = method
                };
                var response = await request.SendRequestAsync(content, CancellationToken.None, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                var errorContent = new StringContent(ex.Message);
                var errorResponse = new HttpResponseMessage
                {
                    Content = errorContent,
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
                return errorResponse;

            }
        }

    }

}
