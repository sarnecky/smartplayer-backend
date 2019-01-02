using Smartplayer.Authorization.WebApi.Models.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Common;

namespace Smartplayer.Authorization.WebApi.Data.Migrations
{
    public class ApplicationUserClub : IAggregate
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int ClubId { get; set; }
        public Club Club { get; set; }
    }
}
