using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Field
{
    public class Field
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool Private { get; set; }
        public string JSONCoordinates { get; set; }
        public virtual Club.Club Club { get; set; }
    }
}
