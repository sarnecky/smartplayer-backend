﻿using Smartplayer.Authorization.WebApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Models.Field
{
    public class Field : IAggregate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool Private { get; set; }
        public string JSONCoordinates { get; set; }
        public int ClubId { get; set; }
        public virtual Club.Club Club { get; set; }
    }
}
