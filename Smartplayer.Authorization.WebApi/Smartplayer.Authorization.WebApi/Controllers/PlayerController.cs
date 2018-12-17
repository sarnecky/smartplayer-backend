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
    public class PlayerController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerTeamRepository _playerTeamRepository;
        private readonly ITeamRepository _teamRepository;

        public PlayerController(
            ILogger<PlayerController> logger, 
            IPlayerRepository playerRepository,
            IPlayerTeamRepository playerTeamRepository,
            ITeamRepository teamRepository)
        {
            _logger = logger;
            _playerRepository = playerRepository;
            _playerTeamRepository = playerTeamRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Creating new player
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody]DTO.Player.Input.Player player)
        {
            var playerResult = await _playerRepository.AddAsync(new Models.Player.Player()
            {
                DateOfBirth = player.DateOfBirth,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Growth = player.Growth,
                Weight = player.Weight
            });

            var result = AutoMapper.Mapper.Map<DTO.Player.Output.Player>(playerResult);
            return Ok(result);
        }

        [HttpPost("addPlayerToTeam")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AddPlayerToTeam(string playerId, string teamId)
        {
            if (!int.TryParse(playerId, out int id) || !int.TryParse(teamId, out int assignedTeamId))
                return BadRequest("Player and Team id shoud be integer");

            var playerResult = await _playerTeamRepository.AddAsync(new Models.Player.PlayerTeam()
            {
                PlayerId = id,
                TeamId = assignedTeamId
            });

            return playerResult != null
                ? Ok(true)
                : Ok(false);
        }

        /// <summary>
        /// Return details for club
        /// </summary>
        /// <returns></returns>
        [HttpGet("details/{playerId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDetails(string playerId)
        {
            if (!int.TryParse(playerId, out int id))
                return BadRequest("Id shoud be integer");

            var playerResult = await _playerRepository.FindById(id);

            var result = AutoMapper.Mapper.Map<DTO.Player.Output.Player>(playerResult);
            return Ok(result);
        }

        /// <summary>
        /// Filtered list of playes for team
        /// </summary>
        /// <returns></returns>
        [HttpGet("listOfPlayersForClub/{clubId}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetPlayers(string clubId)
        {
            if (!int.TryParse(clubId, out int id))
                return BadRequest("Id shoud be integer");

            var teams = await _teamRepository.FindByCriteriaWithPlayerTeams(i => i.ClubId == id);
            var playerTeams = GetPlayerTeamsForAllTeams(teams).ToList();
            var players = GetPlayers(playerTeams).ToList();
            var mappedPlayers = AutoMapper.Mapper.Map<IList<DTO.Player.Output.Player>>(players);
            return Ok(mappedPlayers);
        }

        private IEnumerable<Models.Player.PlayerTeam> GetPlayerTeamsForAllTeams(IList<Models.Team.Team> teams) =>
            teams.SelectMany(i => i.PlayerTeams.ToList());

        private IEnumerable<Models.Player.Player> GetPlayers(IList<Models.Player.PlayerTeam> playerTeams)
        {
            foreach (var playerTeam in playerTeams)
            {
                yield return _playerRepository.FindById(playerTeam.PlayerId).GetAwaiter().GetResult(); ;
            }
        }

    }
}
