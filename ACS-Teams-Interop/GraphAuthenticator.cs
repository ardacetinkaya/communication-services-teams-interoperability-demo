using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;

namespace ACS_Teams_Interop
{
    public interface IGraphAuthenticator
    {
        GraphServiceClient GetAuthenticatedClient();
    }

    public enum AuthenticatorProvider
    {
        PublicClient,
        ConfidentialClient
    }

    public class GraphAuthenticator : IGraphAuthenticator
    {
        private readonly string _clientId = string.Empty;
        private readonly string _clientSecret = string.Empty;
        private readonly string _tenantId = string.Empty;
        private readonly string _redirectUri = string.Empty;
        private readonly string[] _graphScopes = null;
        private readonly AuthenticatorProvider _authenticatorProvider;
        private readonly SecureString _password = null;
        private readonly string _username = string.Empty;

        public GraphAuthenticator(string clientId, string clientSecret, string tenantId, string redirectUri, string graphScopes
            , AuthenticatorProvider provider = AuthenticatorProvider.ConfidentialClient, string username = "", SecureString password = null)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _redirectUri = redirectUri;
            _graphScopes = graphScopes.Split(';');
            _authenticatorProvider = provider;
            _password = password;
            _username = username;

            if (_authenticatorProvider == AuthenticatorProvider.PublicClient)
            {
                if (string.IsNullOrEmpty(_username) || _password == null)
                {
                    throw new ArgumentNullException($"{nameof(username)} || {nameof(password)}");
                }
            }
        }
        public GraphServiceClient GetAuthenticatedClient()
        {
            GraphServiceClient client = null;
            switch (_authenticatorProvider)
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
                case AuthenticatorProvider.ConfidentialClient:
                    client = new GraphServiceClient(new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var clientApplication = ConfidentialClientApplicationBuilder.Create(_clientId)
                            .WithRedirectUri(_redirectUri)
                            .WithClientSecret(_clientSecret)
                            .Build();

                        string[] scopes = new string[] { $"https://graph.microsoft.com/.default" };
                        var accounts = await clientApplication.AcquireTokenForClient(scopes).ExecuteAsync();

                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", accounts.AccessToken);
                    }));
                    break;
            }
            return client;
        }
    }
}
