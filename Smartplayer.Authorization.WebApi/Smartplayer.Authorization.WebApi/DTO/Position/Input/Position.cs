using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class Position
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }
    }
}
