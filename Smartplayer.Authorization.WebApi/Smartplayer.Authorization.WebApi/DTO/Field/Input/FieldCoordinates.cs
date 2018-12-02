using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Field.Input
{
    public class FieldCoordinates
    {
        public Coordinates LeftUp { get; set; }

        public Coordinates LeftDown { get; set; }

        public Coordinates RightUp { get; set; }

        public Coordinates RightDown { get; set; }
    }
}
