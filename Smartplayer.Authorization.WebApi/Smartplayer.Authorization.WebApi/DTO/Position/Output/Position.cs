using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Output
{
    public class Position
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
    }
}
