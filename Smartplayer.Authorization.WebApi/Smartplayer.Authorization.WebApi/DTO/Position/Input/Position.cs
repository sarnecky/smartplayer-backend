using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
