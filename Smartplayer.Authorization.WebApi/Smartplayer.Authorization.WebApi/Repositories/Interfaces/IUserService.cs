using Microsoft.AspNetCore.Identity;
using Smartplayer.Authorization.WebApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByUserName(string username);

        Task<ApplicationUser> GetUserByEmail(string email);

        Task<IdentityResult> CreateUser(ApplicationUser user, string password);

        PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string password);

        Task<IList<Claim>> GetClaims(ApplicationUser user);
    }
}
