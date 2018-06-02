using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Club
{
    public class Club
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public ICollection<Player.Player> Players { get; set; }
        public ICollection<Team.Team> Teams { get; set; }
        public ICollection<Field.Field> Fields { get; set; }
    }
}
