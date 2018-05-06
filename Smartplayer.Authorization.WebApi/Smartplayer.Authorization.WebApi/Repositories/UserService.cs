using Microsoft.AspNetCore.Identity;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Repositories
{
    public class UserService : IUserService
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private PasswordHasher<ApplicationUser> _passwordHasher;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = new PasswordHasher<ApplicationUser>();

        }

        public async Task<ApplicationUser> GetUserByUserName(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityResult> CreateUser(ApplicationUser applicationUser, string password)
        {
            return await _userManager.CreateAsync(applicationUser, password);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> CreateUser(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string password)
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        }

        public async Task<IList<Claim>> GetClaims(ApplicationUser user)
        {
            return await _userManager.GetClaimsAsync(user);
        }

    }
}
