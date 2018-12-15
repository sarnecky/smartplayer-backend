using Smartplayer.Authorization.WebApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Models.Game;

namespace Smartplayer.Authorization.WebApi.Models.Player
{
    public class Player : IAggregate
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public float Growth { get; set; }
        public float Weight { get; set; }
        public ICollection<PlayerTeam> PlayerTeams { get; set; }
        public ICollection<Position> Positions { get; set; }
    }
}
