using Smartplayer.Authorization.WebApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Models.Game;

namespace Smartplayer.Authorization.WebApi.Models.Team
{
    public class Team : IAggregate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Player.PlayerTeam> PlayerTeams { get; set; }
        public ICollection<Game.Game> Games { get; set; }
        public int ClubId { get; set; }
        public virtual Club.Club Club { get; set; }
    }
}
