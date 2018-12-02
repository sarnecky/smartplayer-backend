using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories.PlayerTeam
{
    public class PlayerTeamRepository : BaseRepository<Models.Player.PlayerTeam>, IPlayerTeamRepository
    {
        public PlayerTeamRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
