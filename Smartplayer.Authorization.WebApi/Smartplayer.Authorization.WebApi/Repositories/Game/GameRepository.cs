using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Data;

namespace Smartplayer.Authorization.WebApi.Repositories.Game
{
    public class GameRepository : BaseRepository<Models.Game.Game>, IGameRepository
    {
        public GameRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
