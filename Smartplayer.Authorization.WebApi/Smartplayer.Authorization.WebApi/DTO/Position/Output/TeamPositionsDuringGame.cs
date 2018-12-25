using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Output
{
    public class TeamPositionsDuringGame
    {
        [JsonProperty("players")]
        public IList<Output.PlayerWithPositions> Players { get; set; }
    }
}
