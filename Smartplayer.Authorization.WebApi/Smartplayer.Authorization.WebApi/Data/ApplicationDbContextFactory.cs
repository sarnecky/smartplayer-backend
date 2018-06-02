using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseSqlServer("Data Source = 10.0.75.1; Initial Catalog = SmartPlayer; Persist Security Info = True; User ID = sa; Password = Strong_)Pass");

            return new ApplicationDbContext(builder.Options);
        }
    }
}
