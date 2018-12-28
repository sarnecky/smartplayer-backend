using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Models.Module;
using Smartplayer.Authorization.WebApi.Repositories.Module;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly ILogger _logger;
        private readonly IModuleRepository _moduleRepository;
        public ModuleController(
            ILogger<ModuleController> logger,
            IModuleRepository moduleRepository)
        {
            _logger = logger;
            _moduleRepository = moduleRepository;
        }

        [HttpGet("listOfModules/{clubId}")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetModules(int clubId)
        {
            var modules = await _moduleRepository.FindByCriteria(i => i.ClubId == clubId);
            var response = modules.Select(i => new Smartplayer.Authorization.WebApi.DTO.Module.Output.Module()
            {
                Id = i.Id,
                ClubId = i.ClubId,
                MACAddress = i.MACAddress,
                Name = i.Name
            });

            return Ok(response);
        }


        [HttpPost()]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create(Smartplayer.Authorization.WebApi.DTO.Module.Input.Module module)
        {
            var response = await _moduleRepository.AddAsync(new Module()
            {
                ClubId = module.ClubId,
                MACAddress = module.MACAddress,
                Name = module.Name
            });

            return Ok(true);
        }
    }
}
