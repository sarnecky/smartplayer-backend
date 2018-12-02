using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class TeamController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITeamRepository _teamRepository;

        public TeamController(ILogger<TeamController> logger, ITeamRepository teamRepository)
        {
            _logger = logger;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Creating new team for club
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(200, Type = typeof(DTO.Team.Output.Team))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create(DTO.Team.Input.Team team)
        {
            var teamResult = await _teamRepository.AddAsync(new Models.Team.Team()
            {
                Name = team.Name,
                ClubId = team.ClubId
            });

            var result = AutoMapper.Mapper.Map<DTO.Team.Output.Team>(teamResult);
            return Ok(result);
        }

    }
}
