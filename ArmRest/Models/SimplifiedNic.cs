using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models
{
    public class SimplifiedNic
    {

        public string InternalIpAddress { get; set; }
        public string PublicIpAddress { get; set; }
        public string PublicHostName { get; set; }
    }
}