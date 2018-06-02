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
    public class FieldController : Controller
    {
        private readonly ILogger _logger;

        public FieldController(ILogger<FieldController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creating new field
        /// </summary>
        /// <returns></returns>
        [HttpPost("create/{clubId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create()
        {
            return Ok();
        }

        /// <summary>
        /// List of fields for club
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfFields/{clubId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetListClubs()
        {
            return Ok();
        }
    }
}
