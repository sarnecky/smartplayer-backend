using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.DTO.Position.Input;
using Smartplayer.Authorization.WebApi.DTO.Position.Output;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class GameController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPositionRepository _positionRepository;
        private readonly IPlayerRepository _playerRepository;
        public GameController(
            ILogger<GameController> logger,
            IPositionRepository positionRepository, 
            IPlayerRepository playerRepository)
        {
            _logger = logger;
            _positionRepository = positionRepository;
            _playerRepository = playerRepository;
        }

        [HttpPost("positions/{gameId}/{mapWidth}/{mapHeight}")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetGamePositions(int gameId, int mapWidth, int mapHeight)
        {
            var positions = await _positionRepository.FindByCriteria(i => i.GameId == gameId);
            var groupedPositions = positions.GroupBy(i => i.PlayerId);

            var playersWithPositions = GetPixelPositions(groupedPositions, mapWidth, mapHeight).ToList();
            return Ok(new TeamPositionsDuringGame()
            {
                Players = playersWithPositions
            });
        }

        private IEnumerable<DTO.Position.Output.PlayerWithPositions> GetPixelPositions(
            IEnumerable<IGrouping<int, Models.Game.Position>> groupedPositions, 
            int mapWidth, 
            int mapHeight)
        {
            foreach (var group in groupedPositions)
            {
                var player = _playerRepository.FindById(group.Key).GetAwaiter().GetResult();
                var playerPositions = group.GetEnumerator();
                yield return new Smartplayer.Authorization.WebApi.DTO.Position.Output.PlayerWithPositions()
                {
                    PlayerName = $"{player.FirstName} {player.LastName}",
                    Positions = PixelPositions(playerPositions, mapWidth, mapHeight).ToList()
                };
            }
        }

        private IEnumerable<DTO.Position.Output.Position> PixelPositions(
            IEnumerator<Models.Game.Position> playerPositions,
            int mapWidth,
            int mapHeight)
        {
            while (playerPositions.MoveNext())
            {
                var pixelPosition = GetPixelPosition(playerPositions.Current, mapWidth, mapHeight);

                yield return new DTO.Position.Output.Position()
                {
                    X = pixelPosition.X,
                    Y = pixelPosition.Y,
                    Date = DateTimeOffset.MaxValue
                };
            }
        }

        private (double X, double Y) GetPixelPosition(
            Models.Game.Position position,
            int mapWidth,
            int mapHeight)
        {


            return (0, 0);
        }

       // private void 

        private (double X, double Y)  GetMeractorXY(
            Models.Game.Position position,
            int mapWidth,
            int mapHeight)
        {
            var x = (position.Lng + 180) * (mapWidth / 360);
            var latRad = position.Lat * Math.PI / 180;
            var mercN = Math.Log(Math.Tan((Math.PI / 4) + (latRad / 2)));
            var y = (mapHeight / 2) - (mapWidth * mercN / 2 * Math.PI);
            return (x, y);
        }

        [HttpPost("createPosition")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody]Positions positions)
        {
            try
            {
                var gameId = positions.GameId;
                foreach (var player in positions.Players)
                {
                    foreach (var position in player.Positions)
                    {
                        await _positionRepository.AddAsync(new Smartplayer.Authorization.WebApi.Models.Game.Position()
                        {
                            Date = position.Date,
                            Lat = position.Latitude,
                            Lng = position.Longitude,
                            GameId = gameId,
                            PlayerId = player.Id
                        });

                    }
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Ok(false);
            }

        }



    }
}
