using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Module
{
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MACAddress { get; set; }
    }
}
