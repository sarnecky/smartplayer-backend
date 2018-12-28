using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smartplayer.Authorization.WebApi.Data;
using Smartplayer.Authorization.WebApi.Services;
using Smartplayer.Authorization.WebApi.Repositories.Interfaces;
using Smartplayer.Authorization.WebApi.Repositories;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Smartplayer.Authorization.WebApi.Repositories.Field;
using Smartplayer.Authorization.WebApi.Repositories.Club;
using Newtonsoft.Json;
using Smartplayer.Authorization.WebApi.DTO.Field.Input;
using Smartplayer.Authorization.WebApi.Repositories.Game;
using Smartplayer.Authorization.WebApi.Repositories.Module;
using Smartplayer.Authorization.WebApi.Repositories.Player;
using Smartplayer.Authorization.WebApi.Repositories.PlayerTeam;
using Smartplayer.Authorization.WebApi.Repositories.Positions;
using Smartplayer.Authorization.WebApi.Repositories.Team;
using Swashbuckle.AspNetCore.Swagger;

namespace Smartplayer.Authorization.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddScoped<IUserService, UserService>(); 
            services.AddTransient<IFieldRepository, FieldRepository>();
            services.AddTransient<IClubRepository, ClubRepository>(); 
            services.AddTransient<IPlayerRepository, PlayerRepository>(); 
            services.AddTransient<IPlayerTeamRepository, PlayerTeamRepository>();
            services.AddTransient<IPositionRepository, PositionRepository>();
            services.AddTransient<ITeamRepository, TeamRepository>(); 
            services.AddTransient<IGameRepository, GameRepository>();
            services.AddTransient<IModuleRepository, ModuleRepository>();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Models.Club.Club, DTO.Club.Output.Club>();
                cfg.CreateMap<Models.Field.Field, DTO.Field.Output.Field>()
                    .ForMember(i => i.FieldCoordinates, o => o.MapFrom(i => JsonConvert.DeserializeObject<FieldCoordinates>(i.JSONCoordinates)));
                cfg.CreateMap<Models.Player.Player, DTO.Player.Output.Player>();
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "API", Version = "v1" });
            });
            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            // global policy - assign here or on each controller
            app.UseCors("CorsPolicy");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
            });
        }
    }
}
