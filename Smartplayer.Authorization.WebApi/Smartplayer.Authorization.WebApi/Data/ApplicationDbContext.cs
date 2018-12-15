using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Smartplayer.Authorization.WebApi.Data.Migrations;
using Smartplayer.Authorization.WebApi.Models.Club;
using Smartplayer.Authorization.WebApi.Models.Field;
using Smartplayer.Authorization.WebApi.Models.Game;
using Smartplayer.Authorization.WebApi.Models.Module;
using Smartplayer.Authorization.WebApi.Models.Player;
using Smartplayer.Authorization.WebApi.Models.Team;

namespace Smartplayer.Authorization.WebApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Club> Clubs { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Module> Module { get; set; }
        public DbSet<Player> Player { get; set; }
        public DbSet<PlayerTeam> PlayerTeam { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<ApplicationUserClub> ApplicationUserClubs { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("User");
            builder.Entity<Club>().ToTable("Club").HasKey(i => i.Id);
            builder.Entity<Club>().HasMany(i => i.Fields).WithOne(i => i.Club);
            builder.Entity<Club>().HasMany(i => i.Teams).WithOne(i => i.Club);

            builder.Entity<Team>().ToTable("Team").HasKey(i => i.Id);
            builder.Entity<Team>().HasOne(i => i.Club).WithMany(i => i.Teams);
            builder.Entity<Team>().HasMany(i => i.Games).WithOne(i => i.Team);
            builder.Entity<Team>().HasMany(i => i.Positions).WithOne(i => i.Team);

            builder.Entity<PlayerTeam>()
                .ToTable("PlayerTeam")
                .HasKey(bc => new { bc.PlayerId, bc.TeamId});

            builder.Entity<PlayerTeam>()
                .HasOne(i => i.Player)
                .WithMany(i => i.PlayerTeams)
                .HasForeignKey(i => i.PlayerId);

            builder.Entity<PlayerTeam>()
                .HasOne(i => i.Team)
                .WithMany(i => i.PlayerTeams)
                .HasForeignKey(i => i.TeamId);

            builder.Entity<ApplicationUserClub>()
                .ToTable("ApplicationUserClub")
                .HasKey(bc => new { bc.ApplicationUserId, bc.ClubId });

            builder.Entity<ApplicationUserClub>()
                .HasOne(i => i.ApplicationUser)
                .WithMany(i => i.ApplicationUserClubs)
                .HasForeignKey(i => i.ApplicationUserId);

            builder.Entity<ApplicationUserClub>()
                .HasOne(i => i.Club)
                .WithMany(i => i.ApplicationUsersClubs)
                .HasForeignKey(i => i.ClubId);

            builder.Entity<Field>().ToTable("Field").HasKey(i => i.Id);
            builder.Entity<Field>().HasOne(i => i.Club).WithMany(i => i.Fields);

            builder.Entity<Module>().ToTable("Module").HasKey(i => i.Id);
            builder.Entity<Module>().HasOne(i => i.Club).WithMany(i => i.Modules);

            builder.Entity<Game>().ToTable("Game").HasKey(i => i.Id);
            builder.Entity<Game>().HasOne(i => i.Team).WithMany(i => i.Games);
            builder.Entity<Game>().HasMany(i => i.Positions).WithOne(i => i.Game);

            builder.Entity<Player>().ToTable("Player").HasKey(i => i.Id);
            builder.Entity<Player>().HasMany(i => i.Positions).WithOne(i => i.Player);

            builder.Entity<Position>().ToTable("Position").HasKey(i => i.Id);
            builder.Entity<Position>().HasOne(i => i.Team).WithMany(i => i.Positions);
            builder.Entity<Position>().HasOne(i => i.Player).WithMany(i => i.Positions);
            builder.Entity<Position>().HasOne(i => i.Game).WithMany(i => i.Positions);

        }
    }
}
