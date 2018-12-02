using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly ILogger _logger;

        public ModuleController(ILogger<ModuleController> logger)
        {
            _logger = logger;
        }
    }
}
