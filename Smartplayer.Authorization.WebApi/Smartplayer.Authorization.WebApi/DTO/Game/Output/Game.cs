using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Game.Output
{
    public class Game
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public string Opponent { get; set; }
        public int TeamId { get; set; }
        public int FieldId { get; set; }
    }
}
