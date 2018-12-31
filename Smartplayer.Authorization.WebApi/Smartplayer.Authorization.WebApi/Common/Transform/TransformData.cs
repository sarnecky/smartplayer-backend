using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Smartplayer.Authorization.WebApi.Common.Transform
{
    public class TransformData
    {
        public double ScaleX { get; set; }
        public double ScaleY { get; set; }
        public double StartXOffset { get; set; }
        public double StartYOffset { get; set; }
        public double DegreeOffset { get; set; }
        public double DegreeSinRadOffset { get; set; }
        public double DegreeCosRadOffset { get; set; }
    }
}
