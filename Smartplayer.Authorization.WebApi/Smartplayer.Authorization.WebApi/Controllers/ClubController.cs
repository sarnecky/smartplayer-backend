using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ClubController : Controller
    {
        private readonly ILogger _logger;
        private readonly IClubRepository _clubRepository;
        public ClubController(
            ILogger<ClubController> logger,
            IClubRepository clubRepository)
        {
            _logger = logger;
            _clubRepository = clubRepository;
        }

        /// <summary>
        /// Creating new club
        /// </summary>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody]DTO.Club.Input.Club club)
        {
            var clubResult = await _clubRepository.AddAsync(new Models.Club.Club()
            {
                FullName = club.FullName
            });

            var result = AutoMapper.Mapper.Map<DTO.Club.Output.Club>(clubResult);
            return Ok(result);
        }
    }
}
