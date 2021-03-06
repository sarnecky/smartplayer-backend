﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Data.Migrations;
using Smartplayer.Authorization.WebApi.Models;
using Smartplayer.Authorization.WebApi.Repositories.ApplicationUserClub;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using Smartplayer.Common.Authorization;
using Smartplayer.Common.Base64;
using Smartplayer.Common.ResponseToFrontend;

namespace Smartplayer.Authorization.WebApi.Controllers
{
    /// <summary>
    /// Controller for Users Authorization
    /// </summary>
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private ApplicationDbContext _appContext;
        private IConfiguration _configuration { get; }
        private readonly IApplicationUserClubRepository _applicationUserClubRepository;
        private readonly IClubRepository _clubRepository;
        public UserController(
            ApplicationDbContext appContext,
            SignInManager<ApplicationUser> signInManager,
            ILogger<TeamController> logger,
            IUserService userService,
            IConfiguration configuration,
            IApplicationUserClubRepository applicationUserClubRepository,
            IClubRepository clubRepository)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
            _applicationUserClubRepository = applicationUserClubRepository;
            _clubRepository = clubRepository;
            _appContext = appContext;
        }

        [HttpPost("token")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Token([FromBody]LoginData loginData)
        {
            var user = await _userService.GetUserByEmail(loginData.Email);
            if (user == null)
                return BadRequest(ResponseMessage.UserNotExsits());

            if(!_userService.VerifyHashedPassword(user, loginData.Password).Equals(PasswordVerificationResult.Success))
                return BadRequest(ResponseMessage.UserNotExsits());

            var club = (await _applicationUserClubRepository.FindByCriteria(i => i.ApplicationUserId == user.Id)).FirstOrDefault();
            
            return Ok(await GenerateTokens(user, club));
        }

        [HttpPost("refreshtoken")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RefreshToken([FromBody]RefreshTokenData refreshTokenData)
        {
            if (refreshTokenData == null || string.IsNullOrEmpty(refreshTokenData.RefreshToken))
                return BadRequest(ResponseMessage.ProblemWithRefreshToken("Refresh token is empty"));

            var refreshToken = Base64Converter.Base64Decode(refreshTokenData.RefreshToken);

            var dataForRefresh = refreshToken.Split("-");
            if(!dataForRefresh.Any())
                return BadRequest(ResponseMessage.ProblemWithRefreshToken("Token is in bad format"));

            var user = await _userService.GetUserByEmail(dataForRefresh[1]);
            var club = (await _applicationUserClubRepository.FindByCriteria(i => i.ApplicationUserId == user.Id)).FirstOrDefault();

            return Ok(await GenerateTokens(user, club));
        }

        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Register(SignUpRequest signUpRequest)
        {
            var user = await _userService.GetUserByEmail(signUpRequest.Email);
            if(user!=null)
                return BadRequest(ResponseMessage.UserExsits());

            var newUser = new ApplicationUser()
            {
                UserName = $"{signUpRequest.FirstName}",
                Email = signUpRequest.Email,
            };

            var createdUser = await _userService.CreateUser(newUser, signUpRequest.Password);
            if (!createdUser.Succeeded)
                return BadRequest(createdUser.Errors);

            var clubResult = await _clubRepository.AddAsync(new Models.Club.Club()
            {
                FullName = signUpRequest.ClubName
            });

            var userhehe = await _userService.GetUserByEmail(signUpRequest.Email);

            var applicationUserInClub = await _applicationUserClubRepository.AddAsync(new ApplicationUserClub()
            {
                ApplicationUserId = userhehe.Id,
                ClubId = clubResult.Id
            });

            _logger.LogInformation($"User created a new account with email: {signUpRequest.Email}");

            return Ok();
        }

        private async Task<LoginRepsonse> GenerateTokens(ApplicationUser user, ApplicationUserClub applicationUserClub)
        {
            var userClaims = await _userService.GetClaims(user);
            var jti = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("userId", user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, jti), //unique identifier for JWT
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
             }.Union(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Tokens:Issuer"],
                audience: _configuration["Tokens:Issuer"],
                claims: claims,
                expires:  DateTime.UtcNow.AddDays(14),
                signingCredentials: creds);

            var refresh = Base64Converter.Base64Encode($"{jti}-{user.UserName}");
            user.RefreshToken = refresh;
            _appContext.Update(user);
            await _appContext.SaveChangesAsync();

            return new LoginRepsonse()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refresh, 
                Expiration = token.ValidTo,
                ClubId = applicationUserClub?.ClubId ?? 0,
                UserName = user.UserName
            };
        }
    }
}