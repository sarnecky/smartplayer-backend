using Smartplayer.Authorization.WebApi.Common;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Smartplayer.Authorization.WebApi.Repositories.Team
{
    public class TeamRepository : BaseRepository<Models.Team.Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IList<Models.Team.Team>> FindByCriteriaWithPlayerTeams(Expression<Func<Models.Team.Team, bool>> criteria)
        {
            var result = await _dbSet.AsQueryable().Where(criteria).Include(i => i.PlayerTeams).ToListAsync();
            return result;
        }
    }
}
