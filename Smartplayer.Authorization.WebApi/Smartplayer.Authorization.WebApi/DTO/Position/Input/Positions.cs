using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class Positions
    {
        [JsonProperty("gameId")]
        public int GameId { get; set; }
        [JsonProperty("teamId")]
        public int TeamId { get; set; }
        [JsonProperty("players")]
        public IList<PlayerWithPositions> Players { get; set; }
    }
}
