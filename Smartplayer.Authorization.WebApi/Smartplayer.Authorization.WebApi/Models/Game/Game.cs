using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Common;

namespace Smartplayer.Authorization.WebApi.Models.Game
{
    public class Game : IAggregate
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public string Opponent { get; set; }
        public int TeamId { get; set; }
        public virtual Team.Team Team { get; set; }
        public int? WhereId { get; set; }
        public ICollection<Position> Positions { get; set; }

    }
}
