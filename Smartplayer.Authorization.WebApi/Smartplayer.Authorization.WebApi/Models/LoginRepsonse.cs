using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models
{
    public class LoginRepsonse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public int ClubId { get; set; }
        public string UserName { get; set; }
    }
}
