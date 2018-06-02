using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Team
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Player.PlayerTeam> PlayerTeams { get; set; }
        public virtual Club.Club Club { get; set; }
    }
}
