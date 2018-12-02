using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories.Interfaces
{
    public interface ITeamRepository : IRepository<Models.Team.Team>
    {
        Task<IList<Models.Team.Team>> FindByCriteriaWithPlayerTeams(Expression<Func<Models.Team.Team, bool>> criteria);
    }
}
