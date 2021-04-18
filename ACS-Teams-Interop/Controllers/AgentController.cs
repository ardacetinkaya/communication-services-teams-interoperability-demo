using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACS_Teams_Interop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly ILogger<AgentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _agentId = string.Empty;
        private readonly string _meetingLink = string.Empty;
        public AgentController(ILogger<AgentController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _meetingLink = _configuration["Agent:MeetingLink"];
            _agentId = _configuration["Agent:Id"];
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(new
            {
                MeetingLink = _meetingLink,
                AgentId = _agentId
            });
        }
    }
}
