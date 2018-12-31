using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Output
{
    public class PlayerWithPositions
    {
        [JsonProperty("playerName")]
        public string PlayerName { get; set; }
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }
        [JsonProperty("positions")]
        public IList<Output.Position> Positions { get; set; }
    }
}
