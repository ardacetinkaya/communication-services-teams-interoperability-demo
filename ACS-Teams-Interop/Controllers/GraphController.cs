using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ACS_Teams_Interop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly GraphAuthenticator _graphAuthenticator;
        private readonly GraphService _graphService;
        public GraphController(GraphAuthenticator authenticator, GraphService service)
        {
            _graphAuthenticator = authenticator;
            _graphService = service;

            _graphService.Authenticate(_graphAuthenticator);
        }

        [HttpGet]
        [Route("{*url}")]
        public async Task<IActionResult> GetAsync(string url)
        {
            var response = await _graphService.ProcessRequestAsync("GET", url, null, string.Empty, Request.ContentType);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<object>(responseData);
                return Ok(new
                {
                    Data = data
                });
            }

            return BadRequest();

        }
    }
}
