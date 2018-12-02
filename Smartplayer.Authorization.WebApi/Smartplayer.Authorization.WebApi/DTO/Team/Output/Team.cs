using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Team.Output
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClubId { get; set; }
    }
}
