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
using PlayerWithPositions = Smartplayer.Authorization.WebApi.DTO.Position.Input.PlayerWithPositions;
using Position = Smartplayer.Authorization.WebApi.DTO.Position.Input.Position;

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
                    Date = playerPositions.Current.Date.ToString()
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

        [HttpPost("generate")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> gen()
        {
            var positions = new Positions();
            positions.GameId = 2;
            positions.Players = new List<PlayerWithPositions>()
            {
                
                new PlayerWithPositions()
                {
                    Id = 2,
                    Positions = new List<Position>()
                    {
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:00.8460000 +01:00"), Longitude = double.Parse("18.6301145"), Latitude = double.Parse("54.3702088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:01.8460000 +01:00"), Longitude = double.Parse("18.6301306"), Latitude = double.Parse("54.3702073")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:02.8460000 +01:00"), Longitude = double.Parse("18.6301426"), Latitude = double.Parse("54.3702049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:03.8460000 +01:00"), Longitude = double.Parse("18.6301574"), Latitude = double.Parse("54.3702018")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:04.8460000 +01:00"), Longitude = double.Parse("18.6301721"), Latitude = double.Parse("54.3701987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:05.8460000 +01:00"), Longitude = double.Parse("18.6301842"), Latitude = double.Parse("54.3701948")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:06.8460000 +01:00"), Longitude = double.Parse("18.6302017"), Latitude = double.Parse("54.3701916")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:07.8460000 +01:00"), Longitude = double.Parse("18.6302231"), Latitude = double.Parse("54.3701877")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:08.8460000 +01:00"), Longitude = double.Parse("18.6302379"), Latitude = double.Parse("54.3701838")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:09.8460000 +01:00"), Longitude = double.Parse("18.6302526"), Latitude = double.Parse("54.3701791")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:10.8460000 +01:00"), Longitude = double.Parse("18.6302687"), Latitude = double.Parse("54.3701745")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:11.8460000 +01:00"), Longitude = double.Parse("18.6302861"), Latitude = double.Parse("54.370169")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:12.8460000 +01:00"), Longitude = double.Parse("18.6303063"), Latitude = double.Parse("54.3701627")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:13.8460000 +01:00"), Longitude = double.Parse("18.6303224"), Latitude = double.Parse("54.3701557")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:14.8460000 +01:00"), Longitude = double.Parse("18.6303358"), Latitude = double.Parse("54.3701534")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:15.8460000 +01:00"), Longitude = double.Parse("18.6303572"), Latitude = double.Parse("54.3701448")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:16.8460000 +01:00"), Longitude = double.Parse("18.6303814"), Latitude = double.Parse("54.370137")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:17.8460000 +01:00"), Longitude = double.Parse("18.6304001"), Latitude = double.Parse("54.3701276")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:18.8460000 +01:00"), Longitude = double.Parse("18.6304162"), Latitude = double.Parse("54.3701213")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:19.8460000 +01:00"), Longitude = double.Parse("18.6304363"), Latitude = double.Parse("54.370112")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:20.8460000 +01:00"), Longitude = double.Parse("18.6304511"), Latitude = double.Parse("54.3701034")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:21.8460000 +01:00"), Longitude = double.Parse("18.6304752"), Latitude = double.Parse("54.3700948")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:22.8460000 +01:00"), Longitude = double.Parse("18.630494"), Latitude = double.Parse("54.370087")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:23.8460000 +01:00"), Longitude = double.Parse("18.6305101"), Latitude = double.Parse("54.3700784")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:24.8460000 +01:00"), Longitude = double.Parse("18.6305316"), Latitude = double.Parse("54.3700698")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:25.8460000 +01:00"), Longitude = double.Parse("18.630549"), Latitude = double.Parse("54.3700588")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:26.8460000 +01:00"), Longitude = double.Parse("18.6305611"), Latitude = double.Parse("54.370051")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:27.8460000 +01:00"), Longitude = double.Parse("18.6305852"), Latitude = double.Parse("54.3700401")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:28.8460000 +01:00"), Longitude = double.Parse("18.6306013"), Latitude = double.Parse("54.3700299")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:29.8460000 +01:00"), Longitude = double.Parse("18.6306174"), Latitude = double.Parse("54.3700206")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:30.8460000 +01:00"), Longitude = double.Parse("18.6306335"), Latitude = double.Parse("54.3700088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:31.8460000 +01:00"), Longitude = double.Parse("18.6306509"), Latitude = double.Parse("54.3699948")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:32.8460000 +01:00"), Longitude = double.Parse("18.6306469"), Latitude = double.Parse("54.3699768")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:33.8460000 +01:00"), Longitude = double.Parse("18.6306455"), Latitude = double.Parse("54.369944")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:34.8460000 +01:00"), Longitude = double.Parse("18.6306643"), Latitude = double.Parse("54.3699088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:35.8460000 +01:00"), Longitude = double.Parse("18.6306831"), Latitude = double.Parse("54.369876")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:36.8460000 +01:00"), Longitude = double.Parse("18.6307032"), Latitude = double.Parse("54.3698338")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:37.8460000 +01:00"), Longitude = double.Parse("18.6307273"), Latitude = double.Parse("54.3697916")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:38.8460000 +01:00"), Longitude = double.Parse("18.6307367"), Latitude = double.Parse("54.3697526")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:39.8460000 +01:00"), Longitude = double.Parse("18.6307287"), Latitude = double.Parse("54.369726")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:40.8460000 +01:00"), Longitude = double.Parse("18.6307058"), Latitude = double.Parse("54.3697034")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:41.8460000 +01:00"), Longitude = double.Parse("18.6306763"), Latitude = double.Parse("54.3696972")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:42.8460000 +01:00"), Longitude = double.Parse("18.6306602"), Latitude = double.Parse("54.369687")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:43.8460000 +01:00"), Longitude = double.Parse("18.6306454"), Latitude = double.Parse("54.3696784")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:44.8460000 +01:00"), Longitude = double.Parse("18.6306387"), Latitude = double.Parse("54.3696729")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:45.8460000 +01:00"), Longitude = double.Parse("18.6306266"), Latitude = double.Parse("54.3696706")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:46.8460000 +01:00"), Longitude = double.Parse("18.630624"), Latitude = double.Parse("54.3696792")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:47.8460000 +01:00"), Longitude = double.Parse("18.6306253"), Latitude = double.Parse("54.369687")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:48.8460000 +01:00"), Longitude = double.Parse("18.6306374"), Latitude = double.Parse("54.3696839")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:49.8460000 +01:00"), Longitude = double.Parse("18.6306374"), Latitude = double.Parse("54.3696839")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:50.8460000 +01:00"), Longitude = double.Parse("18.6306374"), Latitude = double.Parse("54.369694")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:51.8460000 +01:00"), Longitude = double.Parse("18.6306307"), Latitude = double.Parse("54.369701")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:52.8460000 +01:00"), Longitude = double.Parse("18.6306159"), Latitude = double.Parse("54.3696963")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:53.8460000 +01:00"), Longitude = double.Parse("18.6306106"), Latitude = double.Parse("54.3697049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:54.8460000 +01:00"), Longitude = double.Parse("18.6306266"), Latitude = double.Parse("54.3697073")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:55.8460000 +01:00"), Longitude = double.Parse("18.6306414"), Latitude = double.Parse("54.3697065")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:56.8460000 +01:00"), Longitude = double.Parse("18.6306293"), Latitude = double.Parse("54.3697182")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:57.8460000 +01:00"), Longitude = double.Parse("18.6306173"), Latitude = double.Parse("54.3697237")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:58.8460000 +01:00"), Longitude = double.Parse("18.6306065"), Latitude = double.Parse("54.3697291")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:00:59.8460000 +01:00"), Longitude = double.Parse("18.6305931"), Latitude = double.Parse("54.3697338")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:00.8460000 +01:00"), Longitude = double.Parse("18.630581"), Latitude = double.Parse("54.3697401")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:01.8460000 +01:00"), Longitude = double.Parse("18.6305676"), Latitude = double.Parse("54.3697463")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:02.8460000 +01:00"), Longitude = double.Parse("18.6305542"), Latitude = double.Parse("54.3697534")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:03.8460000 +01:00"), Longitude = double.Parse("18.6305408"), Latitude = double.Parse("54.3697612")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:04.8460000 +01:00"), Longitude = double.Parse("18.6305261"), Latitude = double.Parse("54.3697682")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:05.8460000 +01:00"), Longitude = double.Parse("18.63051"), Latitude = double.Parse("54.3697745")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:06.8460000 +01:00"), Longitude = double.Parse("18.6304952"), Latitude = double.Parse("54.3697807")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:07.8460000 +01:00"), Longitude = double.Parse("18.6304818"), Latitude = double.Parse("54.369787")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:08.8460000 +01:00"), Longitude = double.Parse("18.6304617"), Latitude = double.Parse("54.3697924")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:09.8460000 +01:00"), Longitude = double.Parse("18.6304496"), Latitude = double.Parse("54.3698002")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:10.8460000 +01:00"), Longitude = double.Parse("18.6304322"), Latitude = double.Parse("54.3698104")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:11.8460000 +01:00"), Longitude = double.Parse("18.6304201"), Latitude = double.Parse("54.3698182")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:12.8460000 +01:00"), Longitude = double.Parse("18.6304013"), Latitude = double.Parse("54.3698276")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:13.8460000 +01:00"), Longitude = double.Parse("18.6303866"), Latitude = double.Parse("54.3698354")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:14.8460000 +01:00"), Longitude = double.Parse("18.6303732"), Latitude = double.Parse("54.369844")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:15.8460000 +01:00"), Longitude = double.Parse("18.6303517"), Latitude = double.Parse("54.3698526")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:16.8460000 +01:00"), Longitude = double.Parse("18.6303396"), Latitude = double.Parse("54.3698581")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:17.8460000 +01:00"), Longitude = double.Parse("18.6303276"), Latitude = double.Parse("54.3698659")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:18.8460000 +01:00"), Longitude = double.Parse("18.6303155"), Latitude = double.Parse("54.3698721")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:19.8460000 +01:00"), Longitude = double.Parse("18.6303048"), Latitude = double.Parse("54.3698799")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:20.8460000 +01:00"), Longitude = double.Parse("18.630294"), Latitude = double.Parse("54.3698838")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:21.8460000 +01:00"), Longitude = double.Parse("18.6302766"), Latitude = double.Parse("54.3698885")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:22.8460000 +01:00"), Longitude = double.Parse("18.6302645"), Latitude = double.Parse("54.3698948")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:23.8460000 +01:00"), Longitude = double.Parse("18.6302565"), Latitude = double.Parse("54.369901")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:24.8460000 +01:00"), Longitude = double.Parse("18.6302471"), Latitude = double.Parse("54.3699088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:25.8460000 +01:00"), Longitude = double.Parse("18.6302364"), Latitude = double.Parse("54.3699166")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:26.8460000 +01:00"), Longitude = double.Parse("18.6302391"), Latitude = double.Parse("54.369926")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:27.8460000 +01:00"), Longitude = double.Parse("18.6302458"), Latitude = double.Parse("54.3699385")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:28.8460000 +01:00"), Longitude = double.Parse("18.6302511"), Latitude = double.Parse("54.3699463")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:29.8460000 +01:00"), Longitude = double.Parse("18.6302377"), Latitude = double.Parse("54.369951")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:30.8460000 +01:00"), Longitude = double.Parse("18.6302203"), Latitude = double.Parse("54.3699557")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:31.8460000 +01:00"), Longitude = double.Parse("18.6301975"), Latitude = double.Parse("54.3699596")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:32.8460000 +01:00"), Longitude = double.Parse("18.6301801"), Latitude = double.Parse("54.3699612")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:33.8460000 +01:00"), Longitude = double.Parse("18.6301666"), Latitude = double.Parse("54.3699651")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:34.8460000 +01:00"), Longitude = double.Parse("18.6301546"), Latitude = double.Parse("54.3699713")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:35.8460000 +01:00"), Longitude = double.Parse("18.6301479"), Latitude = double.Parse("54.3699799")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:36.8460000 +01:00"), Longitude = double.Parse("18.6301412"), Latitude = double.Parse("54.369987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:37.8460000 +01:00"), Longitude = double.Parse("18.6301318"), Latitude = double.Parse("54.3699956")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:38.8460000 +01:00"), Longitude = double.Parse("18.6301304"), Latitude = double.Parse("54.3700026")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:39.8460000 +01:00"), Longitude = double.Parse("18.6301573"), Latitude = double.Parse("54.3700065")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:39.8460000 +01:00"), Longitude = double.Parse("18.6301787"), Latitude = double.Parse("54.3699979")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:40.8460000 +01:00"), Longitude = double.Parse("18.6301948"), Latitude = double.Parse("54.3699909")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:41.8460000 +01:00"), Longitude = double.Parse("18.6302176"), Latitude = double.Parse("54.3699909")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:42.8460000 +01:00"), Longitude = double.Parse("18.6301465"), Latitude = double.Parse("54.3699979")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:43.8460000 +01:00"), Longitude = double.Parse("18.630168"), Latitude = double.Parse("54.3699877")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:44.8460000 +01:00"), Longitude = double.Parse("18.6301921"), Latitude = double.Parse("54.3699846")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:45.8460000 +01:00"), Longitude = double.Parse("18.6302431"), Latitude = double.Parse("54.3699799")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:46.8460000 +01:00"), Longitude = double.Parse("18.6302713"), Latitude = double.Parse("54.3699854")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:47.8460000 +01:00"), Longitude = double.Parse("18.6302873"), Latitude = double.Parse("54.3699909")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:48.8460000 +01:00"), Longitude = double.Parse("18.630294"), Latitude = double.Parse("54.3700041")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:49.8460000 +01:00"), Longitude = double.Parse("18.6303075"), Latitude = double.Parse("54.3700213")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:50.8460000 +01:00"), Longitude = double.Parse("18.6303101"), Latitude = double.Parse("54.3700315")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:51.8460000 +01:00"), Longitude = double.Parse("18.6303115"), Latitude = double.Parse("54.3700448")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:52.8460000 +01:00"), Longitude = double.Parse("18.6303101"), Latitude = double.Parse("54.3700573")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:53.8460000 +01:00"), Longitude = double.Parse("18.6303115"), Latitude = double.Parse("54.370069")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:54.8460000 +01:00"), Longitude = double.Parse("18.6303155"), Latitude = double.Parse("54.3700815")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:55.8460000 +01:00"), Longitude = double.Parse("18.6303168"), Latitude = double.Parse("54.3700909")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:56.8460000 +01:00"), Longitude = double.Parse("18.6303262"), Latitude = double.Parse("54.3700987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:57.8460000 +01:00"), Longitude = double.Parse("18.6303329"), Latitude = double.Parse("54.3701088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:58.8460000 +01:00"), Longitude = double.Parse("18.6303504"), Latitude = double.Parse("54.3701049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:01:59.8460000 +01:00"), Longitude = double.Parse("18.6303598"), Latitude = double.Parse("54.3700932")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:00.8460000 +01:00"), Longitude = double.Parse("18.6303732"), Latitude = double.Parse("54.3700799")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:01.8460000 +01:00"), Longitude = double.Parse("18.6303987"), Latitude = double.Parse("54.3700581")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:02.8460000 +01:00"), Longitude = double.Parse("18.6304121"), Latitude = double.Parse("54.3700456")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:03.8460000 +01:00"), Longitude = double.Parse("18.6304362"), Latitude = double.Parse("54.370026")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:04.8460000 +01:00"), Longitude = double.Parse("18.6304536"), Latitude = double.Parse("54.3700096")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:05.8460000 +01:00"), Longitude = double.Parse("18.6304805"), Latitude = double.Parse("54.3699885")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:06.8460000 +01:00"), Longitude = double.Parse("18.6304966"), Latitude = double.Parse("54.3699721")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:07.8460000 +01:00"), Longitude = double.Parse("18.630514"), Latitude = double.Parse("54.3699495")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:08.8460000 +01:00"), Longitude = double.Parse("18.6305274"), Latitude = double.Parse("54.369926")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:09.8460000 +01:00"), Longitude = double.Parse("18.6304992"), Latitude = double.Parse("54.3699354")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:10.8460000 +01:00"), Longitude = double.Parse("18.6304858"), Latitude = double.Parse("54.3699432")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:11.8460000 +01:00"), Longitude = double.Parse("18.6304764"), Latitude = double.Parse("54.3699487")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:12.8460000 +01:00"), Longitude = double.Parse("18.6304697"), Latitude = double.Parse("54.3699565")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:13.8460000 +01:00"), Longitude = double.Parse("18.630455"), Latitude = double.Parse("54.3699612")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:14.8460000 +01:00"), Longitude = double.Parse("18.6304416"), Latitude = double.Parse("54.3699651")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:15.8460000 +01:00"), Longitude = double.Parse("18.6304322"), Latitude = double.Parse("54.3699706")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:16.8460000 +01:00"), Longitude = double.Parse("18.6304322"), Latitude = double.Parse("54.3699706")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:17.8460000 +01:00"), Longitude = double.Parse("18.6304295"), Latitude = double.Parse("54.3699784")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:18.8460000 +01:00"), Longitude = double.Parse("18.6304282"), Latitude = double.Parse("54.3699877")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:19.8460000 +01:00"), Longitude = double.Parse("18.6304215"), Latitude = double.Parse("54.3699971")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:20.8460000 +01:00"), Longitude = double.Parse("18.630404"), Latitude = double.Parse("54.370001")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:21.8460000 +01:00"), Longitude = double.Parse("18.630392"), Latitude = double.Parse("54.3699995")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:22.8460000 +01:00"), Longitude = double.Parse("18.6303732"), Latitude = double.Parse("54.3699979")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:23.8460000 +01:00"), Longitude = double.Parse("18.6303531"), Latitude = double.Parse("54.370001")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:24.8460000 +01:00"), Longitude = double.Parse("18.6302927"), Latitude = double.Parse("54.3700096")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:25.8460000 +01:00"), Longitude = double.Parse("18.6302498"), Latitude = double.Parse("54.370019")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:26.8460000 +01:00"), Longitude = double.Parse("18.6302203"), Latitude = double.Parse("54.3700252")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:27.8460000 +01:00"), Longitude = double.Parse("18.6301948"), Latitude = double.Parse("54.3700299")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:28.8460000 +01:00"), Longitude = double.Parse("18.6301774"), Latitude = double.Parse("54.3700362")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:29.8460000 +01:00"), Longitude = double.Parse("18.6301546"), Latitude = double.Parse("54.3700463")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:30.8460000 +01:00"), Longitude = double.Parse("18.6301264"), Latitude = double.Parse("54.3700588")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:31.8460000 +01:00"), Longitude = double.Parse("18.6300929"), Latitude = double.Parse("54.370069")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:32.8460000 +01:00"), Longitude = double.Parse("18.6300929"), Latitude = double.Parse("54.3700557")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:33.8460000 +01:00"), Longitude = double.Parse("18.6300996"), Latitude = double.Parse("54.3700471")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:34.8460000 +01:00"), Longitude = double.Parse("18.6300996"), Latitude = double.Parse("54.3700346")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:35.8460000 +01:00"), Longitude = double.Parse("18.6300996"), Latitude = double.Parse("54.3700221")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:36.8460000 +01:00"), Longitude = double.Parse("18.6300982"), Latitude = double.Parse("54.3700088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:37.8460000 +01:00"), Longitude = double.Parse("18.6301063"), Latitude = double.Parse("54.3699971")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:38.8460000 +01:00"), Longitude = double.Parse("18.6301023"), Latitude = double.Parse("54.3699854")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:39.8460000 +01:00"), Longitude = double.Parse("18.6300969"), Latitude = double.Parse("54.369976")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:40.8460000 +01:00"), Longitude = double.Parse("18.6300956"), Latitude = double.Parse("54.3699659")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:41.8460000 +01:00"), Longitude = double.Parse("18.6300956"), Latitude = double.Parse("54.369951")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:42.8460000 +01:00"), Longitude = double.Parse("18.6300982"), Latitude = double.Parse("54.3699416")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:43.8460000 +01:00"), Longitude = double.Parse("18.630105"), Latitude = double.Parse("54.3699221")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:44.8460000 +01:00"), Longitude = double.Parse("18.6301063"), Latitude = double.Parse("54.3698987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:45.8460000 +01:00"), Longitude = double.Parse("18.630113"), Latitude = double.Parse("54.3698674")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:46.8460000 +01:00"), Longitude = double.Parse("18.6301479"), Latitude = double.Parse("54.3698409")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:47.8460000 +01:00"), Longitude = double.Parse("18.6301881"), Latitude = double.Parse("54.3698237")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:48.8460000 +01:00"), Longitude = double.Parse("18.6302364"), Latitude = double.Parse("54.3698151")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:49.8460000 +01:00"), Longitude = double.Parse("18.6302847"), Latitude = double.Parse("54.3698049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:50.8460000 +01:00"), Longitude = double.Parse("18.6303075"), Latitude = double.Parse("54.3697979")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:51.8460000 +01:00"), Longitude = double.Parse("18.6303504"), Latitude = double.Parse("54.3697862")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:52.8460000 +01:00"), Longitude = double.Parse("18.6303879"), Latitude = double.Parse("54.3697768")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:53.8460000 +01:00"), Longitude = double.Parse("18.6304255"), Latitude = double.Parse("54.3697674")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:54.8460000 +01:00"), Longitude = double.Parse("18.6304657"), Latitude = double.Parse("54.3697573")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:55.8460000 +01:00"), Longitude = double.Parse("18.6304644"), Latitude = double.Parse("54.3697643")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:56.8460000 +01:00"), Longitude = double.Parse("18.6304536"), Latitude = double.Parse("54.3697729")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:57.8460000 +01:00"), Longitude = double.Parse("18.6304362"), Latitude = double.Parse("54.3697815")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:58.8460000 +01:00"), Longitude = double.Parse("18.6304094"), Latitude = double.Parse("54.3697916")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:02:59.8460000 +01:00"), Longitude = double.Parse("18.6303692"), Latitude = double.Parse("54.3698018")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:00.8460000 +01:00"), Longitude = double.Parse("18.630341"), Latitude = double.Parse("54.3698112")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:01.8460000 +01:00"), Longitude = double.Parse("18.630341"), Latitude = double.Parse("54.3698112")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:02.8460000 +01:00"), Longitude = double.Parse("18.6303222"), Latitude = double.Parse("54.3698174")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:03.8460000 +01:00"), Longitude = double.Parse("18.6303182"), Latitude = double.Parse("54.3698245")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:04.8460000 +01:00"), Longitude = double.Parse("18.6302981"), Latitude = double.Parse("54.3698268")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:05.8460000 +01:00"), Longitude = double.Parse("18.6302847"), Latitude = double.Parse("54.3698049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:06.8460000 +01:00"), Longitude = double.Parse("18.6302927"), Latitude = double.Parse("54.3697862")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:07.8460000 +01:00"), Longitude = double.Parse("18.6303249"), Latitude = double.Parse("54.3697721")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:08.8460000 +01:00"), Longitude = double.Parse("18.6303598"), Latitude = double.Parse("54.3697557")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:09.8460000 +01:00"), Longitude = double.Parse("18.6303987"), Latitude = double.Parse("54.3697385")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:10.8460000 +01:00"), Longitude = double.Parse("18.6304389"), Latitude = double.Parse("54.3697229")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:11.8460000 +01:00"), Longitude = double.Parse("18.6304738"), Latitude = double.Parse("54.3697088")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:12.8460000 +01:00"), Longitude = double.Parse("18.630518"), Latitude = double.Parse("54.3696948")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:13.8460000 +01:00"), Longitude = double.Parse("18.630565"), Latitude = double.Parse("54.3696784")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:14.8460000 +01:00"), Longitude = double.Parse("18.6306025"), Latitude = double.Parse("54.3696643")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:15.8460000 +01:00"), Longitude = double.Parse("18.6306159"), Latitude = double.Parse("54.3696705")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:16.8460000 +01:00"), Longitude = double.Parse("18.6306052"), Latitude = double.Parse("54.3696815")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:17.8460000 +01:00"), Longitude = double.Parse("18.6305797"), Latitude = double.Parse("54.3696893")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:18.8460000 +01:00"), Longitude = double.Parse("18.6305556"), Latitude = double.Parse("54.3696932")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:19.8460000 +01:00"), Longitude = double.Parse("18.630522"), Latitude = double.Parse("54.369701")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:20.8460000 +01:00"), Longitude = double.Parse("18.6304818"), Latitude = double.Parse("54.369712")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:21.8460000 +01:00"), Longitude = double.Parse("18.6304523"), Latitude = double.Parse("54.3697221")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:22.8460000 +01:00"), Longitude = double.Parse("18.6304174"), Latitude = double.Parse("54.3697354")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:23.8460000 +01:00"), Longitude = double.Parse("18.630392"), Latitude = double.Parse("54.3697526")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:24.8460000 +01:00"), Longitude = double.Parse("18.6303571"), Latitude = double.Parse("54.3697416")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:25.8460000 +01:00"), Longitude = double.Parse("18.6303383"), Latitude = double.Parse("54.3697346")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:26.8460000 +01:00"), Longitude = double.Parse("18.6303128"), Latitude = double.Parse("54.3697268")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:27.8460000 +01:00"), Longitude = double.Parse("18.6303021"), Latitude = double.Parse("54.369719")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:28.8460000 +01:00"), Longitude = double.Parse("18.6303236"), Latitude = double.Parse("54.3697049")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:29.8460000 +01:00"), Longitude = double.Parse("18.6303598"), Latitude = double.Parse("54.3696963")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:30.8460000 +01:00"), Longitude = double.Parse("18.6304027"), Latitude = double.Parse("54.3696885")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:31.8460000 +01:00"), Longitude = double.Parse("18.6304536"), Latitude = double.Parse("54.3696807")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:32.8460000 +01:00"), Longitude = double.Parse("18.6305167"), Latitude = double.Parse("54.3696666")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:33.8460000 +01:00"), Longitude = double.Parse("18.6305837"), Latitude = double.Parse("54.3696534")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:34.8460000 +01:00"), Longitude = double.Parse("18.630632"), Latitude = double.Parse("54.3696588")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:35.8460000 +01:00"), Longitude = double.Parse("18.6306494"), Latitude = double.Parse("54.3696713")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:36.8460000 +01:00"), Longitude = double.Parse("18.6306468"), Latitude = double.Parse("54.3696846")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:37.8460000 +01:00"), Longitude = double.Parse("18.6306535"), Latitude = double.Parse("54.3696979")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:38.8460000 +01:00"), Longitude = double.Parse("18.6306535"), Latitude = double.Parse("54.369712")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:39.8460000 +01:00"), Longitude = double.Parse("18.6306508"), Latitude = double.Parse("54.3697221")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:40.8460000 +01:00"), Longitude = double.Parse("18.6306441"), Latitude = double.Parse("54.369737")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:41.8460000 +01:00"), Longitude = double.Parse("18.6306387"), Latitude = double.Parse("54.3697487")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:42.8460000 +01:00"), Longitude = double.Parse("18.6306307"), Latitude = double.Parse("54.3697635")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:43.8460000 +01:00"), Longitude = double.Parse("18.6306307"), Latitude = double.Parse("54.3697791")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:44.8460000 +01:00"), Longitude = double.Parse("18.6306226"), Latitude = double.Parse("54.3697924")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:45.8460000 +01:00"), Longitude = double.Parse("18.6306173"), Latitude = double.Parse("54.3698104")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:46.8460000 +01:00"), Longitude = double.Parse("18.6306159"), Latitude = double.Parse("54.3698198")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:47.8460000 +01:00"), Longitude = double.Parse("18.6306159"), Latitude = double.Parse("54.3698307")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:48.8460000 +01:00"), Longitude = double.Parse("18.6306092"), Latitude = double.Parse("54.3698456")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:49.8460000 +01:00"), Longitude = double.Parse("18.6305918"), Latitude = double.Parse("54.3698588")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:50.8460000 +01:00"), Longitude = double.Parse("18.6305717"), Latitude = double.Parse("54.3698713")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:51.8460000 +01:00"), Longitude = double.Parse("18.6305623"), Latitude = double.Parse("54.3698846")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:52.8460000 +01:00"), Longitude = double.Parse("18.6305582"), Latitude = double.Parse("54.3698987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:53.8460000 +01:00"), Longitude = double.Parse("18.6305569"), Latitude = double.Parse("54.3699112")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:54.8460000 +01:00"), Longitude = double.Parse("18.6305515"), Latitude = double.Parse("54.3699284")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:55.8460000 +01:00"), Longitude = double.Parse("18.6305569"), Latitude = double.Parse("54.3699495")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:56.8460000 +01:00"), Longitude = double.Parse("18.6305582"), Latitude = double.Parse("54.369976")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:57.8460000 +01:00"), Longitude = double.Parse("18.6305475"), Latitude = double.Parse("54.3699987")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:58.8460000 +01:00"), Longitude = double.Parse("18.6305354"), Latitude = double.Parse("54.3700237")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:03:59.8460000 +01:00"), Longitude = double.Parse("18.6305354"), Latitude = double.Parse("54.3700362")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:00.8460000 +01:00"), Longitude = double.Parse("18.6305837"), Latitude = double.Parse("54.3700166")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:01.8460000 +01:00"), Longitude = double.Parse("18.6306159"), Latitude = double.Parse("54.3700073")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:02.8460000 +01:00"), Longitude = double.Parse("18.6306401"), Latitude = double.Parse("54.370001")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:03.8460000 +01:00"), Longitude = double.Parse("18.6306924"), Latitude = double.Parse("54.3699877")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:04.8460000 +01:00"), Longitude = double.Parse("18.6307353"), Latitude = double.Parse("54.3699706")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:05.8460000 +01:00"), Longitude = double.Parse("18.6307755"), Latitude = double.Parse("54.3699549")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:06.8460000 +01:00"), Longitude = double.Parse("18.6308171"), Latitude = double.Parse("54.3699385")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:07.8460000 +01:00"), Longitude = double.Parse("18.6308573"), Latitude = double.Parse("54.369912")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:08.8460000 +01:00"), Longitude = double.Parse("18.6308761"), Latitude = double.Parse("54.3698901")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:09.8460000 +01:00"), Longitude = double.Parse("18.6308949"), Latitude = double.Parse("54.3698651")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:10.8460000 +01:00"), Longitude = double.Parse("18.6308895"), Latitude = double.Parse("54.369844")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:11.8460000 +01:00"), Longitude = double.Parse("18.6308828"), Latitude = double.Parse("54.3698276")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:12.8460000 +01:00"), Longitude = double.Parse("18.6308747"), Latitude = double.Parse("54.3698057")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:13.8460000 +01:00"), Longitude = double.Parse("18.6308734"), Latitude = double.Parse("54.3697916")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:14.8460000 +01:00"), Longitude = double.Parse("18.6308546"), Latitude = double.Parse("54.3697752")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:15.8460000 +01:00"), Longitude = double.Parse("18.6308305"), Latitude = double.Parse("54.3697588")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:16.8460000 +01:00"), Longitude = double.Parse("18.6308171"), Latitude = double.Parse("54.369744")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:17.8460000 +01:00"), Longitude = double.Parse("18.6308104"), Latitude = double.Parse("54.3697284")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:18.8460000 +01:00"), Longitude = double.Parse("18.6308131"), Latitude = double.Parse("54.3697143")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:19.8460000 +01:00"), Longitude = double.Parse("18.6307849"), Latitude = double.Parse("54.3697237")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:20.8460000 +01:00"), Longitude = double.Parse("18.6307755"), Latitude = double.Parse("54.3697315")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:21.8460000 +01:00"), Longitude = double.Parse("18.6307889"), Latitude = double.Parse("54.3697338")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:22.8460000 +01:00"), Longitude = double.Parse("18.6307943"), Latitude = double.Parse("54.3697448")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:23.8460000 +01:00"), Longitude = double.Parse("18.6307849"), Latitude = double.Parse("54.3697534")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:24.8460000 +01:00"), Longitude = double.Parse("18.6307701"), Latitude = double.Parse("54.3697448")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:25.8460000 +01:00"), Longitude = double.Parse("18.6307755"), Latitude = double.Parse("54.3697354")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:26.8460000 +01:00"), Longitude = double.Parse("18.6307795"), Latitude = double.Parse("54.3697416")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:27.8460000 +01:00"), Longitude = double.Parse("18.630801"), Latitude = double.Parse("54.3697604")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:28.8460000 +01:00"), Longitude = double.Parse("18.6308064"), Latitude = double.Parse("54.3697471")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:29.8460000 +01:00"), Longitude = double.Parse("18.6308037"), Latitude = double.Parse("54.3697596")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:30.8460000 +01:00"), Longitude = double.Parse("18.6307889"), Latitude = double.Parse("54.3697729")},
new Position(){ Date = DateTimeOffset.Parse("2018-12-17 20:04:31.8460000 +01:00"), Longitude = double.Parse("18.6307648"), Latitude = double.Parse("54.3697643")},


                    }
                }
            };
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
