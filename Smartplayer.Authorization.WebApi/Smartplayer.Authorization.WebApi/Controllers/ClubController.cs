using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ClubController : Controller
    {
        private readonly ILogger _logger;

        public ClubController(ILogger<ClubController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creating new club
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create()
        {
            return Ok();
        }

        /// <summary>
        /// List of clubs for user
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfClubs")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetListClubs()
        {
            return Ok();
        }
    }
}
