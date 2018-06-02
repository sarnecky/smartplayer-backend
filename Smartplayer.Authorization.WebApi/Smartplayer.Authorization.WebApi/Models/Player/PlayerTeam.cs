using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Player
{
    public class PlayerTeam
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public int TeamId { get; set; }
        public virtual Team.Team Team{ get; set; }

    }
}
