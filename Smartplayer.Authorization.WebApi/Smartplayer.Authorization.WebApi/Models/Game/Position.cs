using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Common;

namespace Smartplayer.Authorization.WebApi.Models.Game
{
    public class Position : IAggregate
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player.Player Player { get; set; }
        public int GameId { get; set; }
        public Game Game { get; set; }
        public int TeamId { get; set; }
        public Team.Team Team { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}
