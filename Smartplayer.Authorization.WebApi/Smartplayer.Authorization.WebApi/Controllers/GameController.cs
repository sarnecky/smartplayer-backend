using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smartplayer.Authorization.WebApi.Common.Transform;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.DTO.Field.Input;
using Smartplayer.Authorization.WebApi.DTO.Position.Input;
using Smartplayer.Authorization.WebApi.DTO.Position.Output;
using Smartplayer.Authorization.WebApi.Models.Game;
using Smartplayer.Authorization.WebApi.Repositories.Game;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class GameController : Controller
    {
        private readonly ILogger _logger;
        private readonly IPositionRepository _positionRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ITeamRepository _teamRepository;

        public GameController(
            ILogger<GameController> logger,
            IPositionRepository positionRepository, 
            IPlayerRepository playerRepository,
            IGameRepository gameRepository,
            IFieldRepository fieldRepository,
            ITeamRepository teamRepository)
        {
            _logger = logger;
            _positionRepository = positionRepository;
            _playerRepository = playerRepository;
            _gameRepository = gameRepository;
            _fieldRepository = fieldRepository;
            _teamRepository = teamRepository;
        }

        [HttpPost("createGame")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateGame([FromBody]DTO.Game.Input.Game game)
        {
            var gamedb = await _gameRepository.AddAsync(new Game()
            {
                FieldId = game.FieldId, Host = game.Host, Opponent = game.Opponent, TeamId = game.TeamId
            });

            return Ok(gamedb);
        }

        [HttpGet("games/{clubId}")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetGames(int clubId)
        {
            var teams = await _teamRepository.FindByCriteria(i => i.ClubId == clubId);
            var list = new List<Smartplayer.Authorization.WebApi.DTO.Game.Output.Game>();

            foreach (var team in teams)
            {
                var games = await _gameRepository.FindByCriteria(i => i.TeamId == team.Id);
                foreach (var game in games)
                {
                    list.Add(new Smartplayer.Authorization.WebApi.DTO.Game.Output.Game()
                    {
                        Id = game.Id,
                        Host = game.Host,
                        Opponent = game.Opponent,
                    });
                }
            }
            return Ok(list);
        }

        [HttpPost("positions/{gameId}/{mapWidth}/{mapHeight}")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetGamePositions(int gameId, int mapWidth, int mapHeight)
        {
            var positions = await _positionRepository.FindByCriteria(i => i.GameId == gameId);
            var groupedPositions = positions.GroupBy(i => i.PlayerId);

            var playersWithPositions = GetPixelPositions(groupedPositions, mapWidth, mapHeight, gameId).ToList();
            return Ok(new TeamPositionsDuringGame()
            {
                Players = playersWithPositions
            });
        }

        private IEnumerable<DTO.Position.Output.PlayerWithPositions> GetPixelPositions(
            IEnumerable<IGrouping<int, Models.Game.Position>> groupedPositions, 
            int mapWidth, 
            int mapHeight,
            int gameId)
        {
            var transformData = GetTransformData(gameId, mapWidth, mapHeight).GetAwaiter().GetResult();
            foreach (var group in groupedPositions)
            {
                var player = _playerRepository.FindById(group.Key).GetAwaiter().GetResult();
                var playerPositions = group.GetEnumerator();
                yield return new Smartplayer.Authorization.WebApi.DTO.Position.Output.PlayerWithPositions()
                {
                    PlayerName = $"{player.FirstName} {player.LastName}",
                    PlayerId = player.Id,
                    Positions = PixelPositions(playerPositions, mapWidth, mapHeight, transformData).ToList()
                };
            }
        }

        private IEnumerable<DTO.Position.Output.Position> PixelPositions(
            IEnumerator<Models.Game.Position> playerPositions,
            int mapWidth,
            int mapHeight,
            TransformData transformData)
        {
            while (playerPositions.MoveNext())
            {
                var pixelPosition = GetPixelPosition(playerPositions.Current, mapWidth, mapHeight, transformData);

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
            int mapHeight,
            TransformData transformData)
        {
            var mercatorPosition = GetMeractorXY(position, mapWidth, mapHeight);

            var x = mercatorPosition.X - transformData.StartXOffset;
            var y = mercatorPosition.Y - transformData.StartYOffset;
            
            //var x

            return (0, 0);
        }


        private async Task<TransformData> GetTransformData(
            int gameId,
            int mapWidth,
            int mapHeight)
        {
            var game = await _gameRepository.FindById(gameId);
            var fieldId = game.FieldId.Value;
            var field = await _fieldRepository.FindById(fieldId);
            var fieldCoordinates = JsonConvert.DeserializeObject<FieldCoordinates>(field.JSONCoordinates);

            var leftDown = GetMeractorXY(fieldCoordinates.LeftDown, mapWidth, mapHeight);
            var leftUp = GetMeractorXY(fieldCoordinates.LeftUp, mapWidth, mapHeight);
            var rightDown = GetMeractorXY(fieldCoordinates.LeftDown, mapWidth, mapHeight);
            var rightUp = GetMeractorXY(fieldCoordinates.RightUp, mapWidth, mapHeight);

            return new TransformData()
            {

            };
        }
        
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
        private (double X, double Y) GetMeractorXY(
            Coordinates position,
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
