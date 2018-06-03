using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Smartplayer.Authorization.WebApi.Data.Migrations;
using Smartplayer.Authorization.WebApi.Models.Club;

namespace Smartplayer.Authorization.WebApi.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string RefreshToken { get; set; }
        public ICollection<ApplicationUserClub> ApplicationUserClubs { get; set; }
    }
}
