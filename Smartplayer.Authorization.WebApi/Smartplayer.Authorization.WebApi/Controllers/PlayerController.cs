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
    public class PlayerController : Controller
    {
        private readonly ILogger _logger;

        public PlayerController(ILogger<PlayerController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creating new player
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
        /// Return details for club
        /// </summary>
        /// <returns></returns>
        [HttpGet("details/{playerId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDetails()
        {
            return Ok();
        }

        /// <summary>
        /// Update club details
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
        /// Remove player from club
        /// </summary>
        /// <returns></returns>
        [HttpDelete("removeFromClub/{clubId}/{playerId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Delete()
        {
            return Ok();
        }

        /// <summary>
        /// Remove player from team
        /// </summary>
        /// <returns></returns>
        [HttpDelete("removeFromTeam/{clubId}/{teamId}/{playerId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> DeleteFormTeam()
        {
            return Ok();
        }

        /// <summary>
        /// Filtered list of playes for team
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfPlayersForTeam/{teamId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetPlayers()
        {
            return Ok();
        }
    }
}
