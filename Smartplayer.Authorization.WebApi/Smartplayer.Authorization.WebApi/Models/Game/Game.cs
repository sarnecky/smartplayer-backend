using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Game
{
    public class Game
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public string Opponent { get; set; }
        public int TeamId { get; set; }
        public virtual Team.Team Team { get; set; }
    }
}
