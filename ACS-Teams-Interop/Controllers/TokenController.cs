using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Communication.Identity;
using Microsoft.Extensions.Configuration;
using Azure.Communication;

namespace ACS_Teams_Interop.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString = string.Empty;
        private readonly CommunicationIdentityClient _client = null;

        public TokenController(ILogger<TokenController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            //Azure Communication Services connection string
            _connectionString = _configuration["ConnectionString"];
            _client = new CommunicationIdentityClient(_connectionString);

        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Create token to make Azure Communication Service calls
            var identityResponse = await _client.CreateUserAsync();
            var identity = identityResponse.Value;

            var tokenResponse = await _client.GetTokenAsync(identity, scopes: new[] { CommunicationTokenScope.VoIP });

            return Ok(new
            {
                Token = tokenResponse.Value.Token,
                ExpiresOn = tokenResponse.Value.ExpiresOn,
                Identity = identity.Id
            });
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromQuery]string id)
        {
            //Refresh token for already generated token identifier
            var identityToRefresh = new CommunicationUserIdentifier(id.ToString());
            var tokenResponse = await _client.GetTokenAsync(identityToRefresh, scopes: new[] { CommunicationTokenScope.VoIP });


            return Ok(new
            {
                Token = tokenResponse.Value.Token,
                ExpiresOn = tokenResponse.Value.ExpiresOn,
                Identity = identityToRefresh.Id
            });
        }
    }
}
