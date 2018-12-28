using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Data;

namespace Smartplayer.Authorization.WebApi.Repositories.Module
{
    public class ModuleRepository : BaseRepository<Models.Module.Module>, IModuleRepository
    {
        public ModuleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
