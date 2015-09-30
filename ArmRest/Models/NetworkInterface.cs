using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models.NetworkInterfaceDetails
{


    public class PublicIPAddress
    {
        public string id { get; set; }
    }

    public class Subnet
    {
        public string id { get; set; }
    }

    public class Properties2
    {
        public string provisioningState { get; set; }
        public string privateIPAddress { get; set; }
        public string privateIPAllocationMethod { get; set; }
        public PublicIPAddress publicIPAddress { get; set; }
        public Subnet subnet { get; set; }
    }

    public class IpConfiguration
    {
        public string name { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public Properties2 properties { get; set; }
    }

    public class DnsSettings
    {
        public List<object> dnsServers { get; set; }
        public List<object> appliedDnsServers { get; set; }
    }

    public class NetworkSecurityGroup
    {
        public string id { get; set; }
    }

    public class VirtualMachine
    {
        public string id { get; set; }
    }

    public class Properties
    {
        public string provisioningState { get; set; }
        public string resourceGuid { get; set; }
        public List<IpConfiguration> ipConfigurations { get; set; }
        public DnsSettings dnsSettings { get; set; }
        public bool enableIPForwarding { get; set; }
        public NetworkSecurityGroup networkSecurityGroup { get; set; }
        public VirtualMachine virtualMachine { get; set; }
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