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
    public class ModuleController : Controller
    {
        private readonly ILogger _logger;

        public ModuleController(ILogger<ModuleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creating new module
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
        /// Return details for module
        /// </summary>
        /// <returns></returns>
        [HttpGet("details/{moduleId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDetails()
        {
            return Ok();
        }

        /// <summary>
        /// Update module details
        /// </summary>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Update()
        {
            return Ok();
        }

        /// <summary>
        /// Delete module
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete/{moduleId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Delete()
        {
            return Ok();
        }

        /// <summary>
        /// List of modules
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfModules/{clubId}/{moduleId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetModules()
        {
            return Ok();
        }
    }
}
