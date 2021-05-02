using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Security;

namespace ACS_Teams_Interop
{
    public interface IGraphAuthenticator
    {
        GraphServiceClient GetAuthenticatedClient(AuthenticatorProvider provider, string token);
    }

    public enum AuthenticatorProvider
    {
        PublicClient,
        ConfidentialApplicationClient,
        ConfidentialUserClient
    }

    public class GraphAuthenticator : IGraphAuthenticator
    {
        private readonly string _clientId = string.Empty;
        private readonly string _clientSecret = string.Empty;
        private readonly string _tenantId = string.Empty;
        private readonly string _redirectUri = string.Empty;
        private readonly string _username = string.Empty;
        private readonly SecureString _password = null;
        private readonly string[] _graphScopes = null;
        private readonly string[] _defaultScope = new string[] { $"https://graph.microsoft.com/.default" };

        public GraphAuthenticator(string clientId, string clientSecret, string tenantId, string redirectUri, string graphScopes
            , string username = "", SecureString password = null)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _redirectUri = redirectUri;
            _graphScopes = graphScopes.Split(';');
            _password = password;
            _username = username;

        }
        public GraphServiceClient GetAuthenticatedClient(AuthenticatorProvider provider = AuthenticatorProvider.ConfidentialApplicationClient, string token = "")
        {
            GraphServiceClient client = null;
            switch (provider)
            {
                case AuthenticatorProvider.PublicClient:
                    client = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var clientApplication = PublicClientApplicationBuilder
                            .Create(_clientId)
                            .WithTenantId(_tenantId)
                            .Build();

                        var result = await clientApplication.AcquireTokenByUsernamePassword(_graphScopes
                            , _username
                            , _password).ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
                    break;
                case AuthenticatorProvider.ConfidentialApplicationClient:
                    client = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var clientApplication = ConfidentialClientApplicationBuilder.Create(_clientId)
                            .WithRedirectUri(_redirectUri)
                            .WithTenantId(_tenantId)
                            .WithClientSecret(_clientSecret)
                            .Build();

                        var result = await clientApplication.AcquireTokenForClient(_defaultScope)
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));
                    break;
                case AuthenticatorProvider.ConfidentialUserClient:
                    client = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {

                        var userAssertion = new UserAssertion(token, "urn:ietf:params:oauth:grant-type:jwt-bearer");

                        var clientApplication = ConfidentialClientApplicationBuilder.Create(_clientId)
                             .WithClientSecret(_clientSecret)
                             .WithTenantId(_tenantId)
                             .Build();

                        var result = await clientApplication.AcquireTokenOnBehalfOf(_defaultScope, userAssertion)
                            .ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);

                    }));
                    break;
                default:
                    break;
            }
            return client;
        }
    }
}
