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
    public class GameController : Controller
    {
        private readonly ILogger _logger;

        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creating new game
        /// </summary>
        /// <returns></returns>
        [HttpPost("create/{teamId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create()
        {
            return Ok();
        }

        /// <summary>
        /// Return details for game
        /// </summary>
        /// <returns></returns>
        [HttpGet("details/{gameId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDetails()
        {
            return Ok();
        }

        /// <summary>
        /// Update game details
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
        /// Delete game
        /// </summary>
        /// <returns></returns>
        [HttpDelete("delete/{gameId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Delete()
        {
            return Ok();
        }

        /// <summary>
        /// Filtered list of games for team
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfGamesForTeam/{teamId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetGames()
        {
            return Ok();
        }
    }
}
