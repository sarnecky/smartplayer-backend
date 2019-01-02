using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models
{
    public class SignUpRequest
    {
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ClubName { get; set; }
    }
}
