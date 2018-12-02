using Smartplayer.Authorization.WebApi.DTO.Field.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Field.Output
{
    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool Private { get; set; }
        public FieldCoordinates FieldCoordinates { get; set; }
        public int ClubId { get; set; }
    }
}
