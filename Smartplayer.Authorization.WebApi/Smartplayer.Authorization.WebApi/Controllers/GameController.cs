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

        [HttpGet("positions/{gameId}/{mapWidth}/{mapHeight}")]
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
                    X = (int)pixelPosition.X,
                    Y = (int)pixelPosition.Y,
                    Date = playerPositions.Current.Date
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

            //przeniesienie modelu zeby byl w poczatku ukladu wspolrzednych
            var x = mercatorPosition.X - transformData.StartXOffset;
            var y = mercatorPosition.Y - transformData.StartYOffset;

            //obrót
            var xTurned = x * transformData.DegreeCosRadOffset - y * transformData.DegreeSinRadOffset;
            var yTurned = x * transformData.DegreeSinRadOffset + y * transformData.DegreeCosRadOffset;

            //przemnożenie razy skalę, zeby dostosować do rozmiaru mapy

            var xScaled = xTurned * transformData.ScaleX;
            var yScaled = yTurned * transformData.ScaleX;

            return (xScaled, yScaled);
        }


        private async Task<TransformData> GetTransformData(
            int gameId,
            int mapWidth,
            int mapHeight)
        {
            var transformData = new TransformData();
            var game = await _gameRepository.FindById(gameId);
            var fieldId = game.FieldId.Value;
            var field = await _fieldRepository.FindById(fieldId);
            var fieldCoordinates = JsonConvert.DeserializeObject<FieldCoordinates>(field.JSONCoordinates);

            var leftDown = GetMeractorXY(fieldCoordinates.LeftDown, mapWidth, mapHeight);
            var leftUp = GetMeractorXY(fieldCoordinates.LeftUp, mapWidth, mapHeight);
            var rightDown = GetMeractorXY(fieldCoordinates.RightDown, mapWidth, mapHeight);
            var rightUp = GetMeractorXY(fieldCoordinates.RightUp, mapWidth, mapHeight);

            if (leftUp.Y < leftDown.Y
                && leftUp.Y < rightUp.Y
                && leftUp.Y < rightDown.Y) //najmniejszy jest leftDown
            {

                //dystans z leftDown to rightDown
                var b = ComputeDistance(leftUp.X, leftUp.Y, rightUp.X, rightUp.Y);
                var c = ComputeDistance(leftUp.X, leftUp.Y, rightUp.X, leftUp.Y);
                var rate = c / b;
                var degree = -Math.Acos(rate);
                var degreeCos = Math.Cos(degree);
                var degreeSin = Math.Sin(degree);
                var xOffset = leftDown.X;
                var yOffset = leftDown.Y;

                var lists = TurnPoints(xOffset, yOffset, degreeCos, degreeSin, leftDown, leftUp, rightDown, rightUp);

                var scaleX = mapWidth / lists.XList.Max();
                var scaleY = mapHeight / lists.YList.Max();

                return new TransformData()
                {
                    ScaleY = scaleY,
                    ScaleX = scaleX,
                    StartXOffset = leftUp.X,
                    StartYOffset = leftUp.Y,
                    DegreeCosRadOffset = degreeCos,
                    DegreeSinRadOffset = degreeSin,
                    DegreeOffset = degree
                };
            }

            return new TransformData()
            {
                ScaleY = 0,
                ScaleX = 0,
                StartXOffset = 0,
                StartYOffset = 0,
                DegreeCosRadOffset = 0,
                DegreeSinRadOffset = 0,
                DegreeOffset = 0
            };
        }

        private (List<double> XList, List<double> YList) TurnPoints(
            double xOffset,
            double yOffset,
            double degreeCosRadOffset,
            double degreeSinRadOffset,
            params (double X, double y)[] points)
        {
            var xList = new List<double>();
            var yList = new List<double>();

            foreach (var valueTuple in points)
            {
                var x = valueTuple.X - xOffset;
                var y = valueTuple.y - yOffset;
                xList.Add(x * degreeCosRadOffset - y * degreeSinRadOffset);
                yList.Add(x * degreeSinRadOffset + y * degreeCosRadOffset);
            }

            return (xList, yList);
        }
        private double ComputeDistance(double x1, double y1, double x2, double y2) =>
            Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        
        private (double X, double Y)  GetMeractorXY(
            Models.Game.Position position,
            int mapWidth,
            int mapHeight)
        {
            var x = (position.Lng + 180) * (mapWidth / (double)360);
            var latRad = position.Lat * Math.PI / 180;
            double mercN = Math.Log(Math.Tan((Math.PI / 4) + (latRad / 2)));
            var y = (mapHeight / (double)2) - (mapWidth * mercN / (2 * Math.PI));
            return (x, y);
        }
        private (double X, double Y) GetMeractorXY(
            Coordinates position,
            int mapWidth,
            int mapHeight)
        {
            var x = (position.Lng + 180) * (mapWidth / (double)360);
            var latRad = position.Lat * Math.PI / 180;
            double mercN = Math.Log(Math.Tan((Math.PI / 4) + (latRad / 2)));
            var y = (mapHeight / (double)2) - (mapWidth * mercN /( 2 * Math.PI));
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
