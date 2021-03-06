﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Smartplayer.Authorization.WebApi.Common;

namespace Smartplayer.Authorization.WebApi.Models.Module
{
    public class Module : IAggregate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MACAddress { get; set; }
        public int ClubId { get; set; }
        public virtual Club.Club Club { get; set; }
    }
}
