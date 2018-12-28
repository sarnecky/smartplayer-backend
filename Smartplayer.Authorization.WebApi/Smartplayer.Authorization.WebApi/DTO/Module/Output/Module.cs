using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.DTO.Module.Output
{
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MACAddress { get; set; }
        public int ClubId { get; set; }
    }
}
