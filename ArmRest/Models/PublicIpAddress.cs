using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models.PublicIpAddress
{

    public class DnsSettings
    {
        public string domainNameLabel { get; set; }
        public string fqdn { get; set; }
    }

    public class IpConfiguration
    {
        public string id { get; set; }
    }

    public class Properties
    {
        public string provisioningState { get; set; }
        public string resourceGuid { get; set; }
        public string ipAddress { get; set; }
        public string publicIPAllocationMethod { get; set; }
        public int idleTimeoutInMinutes { get; set; }
        public DnsSettings dnsSettings { get; set; }
        public IpConfiguration ipConfiguration { get; set; }
    }

    public class RootObject
    {
        public string name { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public Properties properties { get; set; }
    }

}