using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;

namespace Smartplayer.Authorization.WebApi.Repositories.ApplicationUserClub
{
    public class ApplicationUserClubRepository : BaseRepository<Data.Migrations.ApplicationUserClub>, IApplicationUserClubRepository
    {
        public ApplicationUserClubRepository(ApplicationDbContext context) : base(context)
        {
        }
    }

    public interface IApplicationUserClubRepository : IRepository<Data.Migrations.ApplicationUserClub>
    {
    }
}
