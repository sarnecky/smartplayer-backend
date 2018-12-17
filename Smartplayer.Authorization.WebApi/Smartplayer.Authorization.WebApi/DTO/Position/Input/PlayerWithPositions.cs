using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class PlayerWithPositions
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("positions")]
        public IList<Position> Positions { get; set; }
    }
}
