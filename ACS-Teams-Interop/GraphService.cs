
using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ACS_Teams_Interop
{
    public class GraphService
    {
        public readonly HttpClient _httpClient;
        private IGraphAuthenticator _graphAuthenticator;

        public GraphService(HttpClient client)
        {
            _httpClient = client;
        }

        public void Authenticate(IGraphAuthenticator authenticator)
        {
            this._graphAuthenticator = authenticator;
        }

        public async Task<HttpResponseMessage> ProcessRequestAsync(string method, string url, object content, string contentType = "application/json")
        {
            try
            {
                var client = _graphAuthenticator.GetAuthenticatedClient();
                var requestUrl = $"{client.BaseUrl.Replace("/v1.0", "")}/{url}";
                var request = new BaseRequest(requestUrl, client, null)
                {
                    Method = method
                };
                var response = await request.SendRequestAsync(content, CancellationToken.None, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                return response;

            }
            catch (Exception ex)
            {
                var errorContent = new StringContent(ex.Message);
                var errorResponse = new HttpResponseMessage
                {
                    Content = errorContent
                };
                return errorResponse;

            }
        }

    }

}
