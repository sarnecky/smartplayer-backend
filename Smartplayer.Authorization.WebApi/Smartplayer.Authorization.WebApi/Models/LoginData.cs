using Smartplayer.Common.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models
{
    public class LoginData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public Platform Platform { get; set; }
    }
}
