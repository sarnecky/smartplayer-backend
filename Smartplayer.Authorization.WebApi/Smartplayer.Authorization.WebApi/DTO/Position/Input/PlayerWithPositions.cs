using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Position.Input
{
    public class PlayerWithPositions
    {
        public int Id { get; set; }
        public IList<Position> Positions { get; set; }
    }
}
