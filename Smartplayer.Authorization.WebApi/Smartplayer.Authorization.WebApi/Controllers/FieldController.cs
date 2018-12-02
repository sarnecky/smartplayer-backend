using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using Newtonsoft.Json;
using Smartplayer.Authorization.WebApi.DTO.Field.Input;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class FieldController : Controller
    {
        private readonly ILogger _logger;
        private readonly IFieldRepository _fieldRepository;

        public FieldController(
            ILogger<FieldController> logger,
            IFieldRepository fieldRepository)
        {
            _logger = logger;
            _fieldRepository = fieldRepository;
        }

        [HttpPost("create")]
        [ProducesResponseType(200, Type = typeof(DTO.Field.Output.Field))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateField(DTO.Field.Input.Field field)
        {
            var clubResult = await _fieldRepository.AddAsync(new Models.Field.Field()
            {
                Name = field.Name,
                JSONCoordinates = JsonConvert.SerializeObject(field.FieldCoordinates),
                Address = field.Address,
                Private = field.Private,
                ClubId = field.ClubId,        
            });

            var result = AutoMapper.Mapper.Map<DTO.Field.Output.Field>(clubResult);

            return Ok(result);
        }

        [HttpGet("listOfFields/{clubId}")]
        [ProducesResponseType(200, Type = typeof(IList<DTO.Field.Output.Field>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetListClubs(string clubId)
        {
            if (!int.TryParse(clubId, out int id))
                return BadRequest("Id shoud be integer");

            var fields = await _fieldRepository.FindByCriteria(i => i.ClubId == id);
            var mappedFields = AutoMapper.Mapper.Map<IList<DTO.Field.Output.Field>>(fields);
            return Ok(mappedFields);
        }
    }
}
