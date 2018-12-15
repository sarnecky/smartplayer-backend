using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class Positions
    {
        public int GameId { get; set; }
        public int TeamId { get; set; }
        public IList<PlayerWithPositions> Players { get; set; }
    }
}
