using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models
{
    public class HardwareProfile
    {
        public string vmSize { get; set; }
    }

    public class ImageReference
    {
        public string publisher { get; set; }
        public string offer { get; set; }
        public string sku { get; set; }
        public string version { get; set; }
    }

    public class Vhd
    {
        public string uri { get; set; }
    }

    public class OsDisk
    {
        public string osType { get; set; }
        public string name { get; set; }
        public string createOption { get; set; }
        public Vhd vhd { get; set; }
        public string caching { get; set; }
    }

    public class StorageProfile
    {
        public ImageReference imageReference { get; set; }
        public OsDisk osDisk { get; set; }
        public List<object> dataDisks { get; set; }
    }

    public class WindowsConfiguration
    {
        public bool provisionVMAgent { get; set; }
        public bool enableAutomaticUpdates { get; set; }
    }

    public class OsProfile
    {
        public string computerName { get; set; }
        public string adminUsername { get; set; }
        public WindowsConfiguration windowsConfiguration { get; set; }
        public List<object> secrets { get; set; }
    }

    public class NicProperties2
    {
    }

    public class NetworkInterface
    {
        public string id { get; set; }
        public NicProperties2 properties { get; set; }
    }

    public class NetworkProfile
    {
        public List<NetworkInterface> networkInterfaces { get; set; }
    }

    public class Properties
    {
        public HardwareProfile hardwareProfile { get; set; }
        public StorageProfile storageProfile { get; set; }
        public OsProfile osProfile { get; set; }
        public NetworkProfile networkProfile { get; set; }
        public string provisioningState { get; set; }
        public InstanceView instanceView { get; set; }
    }

    public class InstanceView
    {
        public List<InstanceViewStatus> statuses { get; set; }
    }

    public class InstanceViewStatus
    {
        public string code { get; set; }
        public string level { get; set; }
        public string displayStatus { get; set; }
        public string time { get; set; }
    }
    public class Resource
    {
        public string id { get; set; }
    }


    public class ComputeVm
    {
        public Properties properties { get; set; }
        public List<Resource> resources { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string location { get; set; }
        public Dictionary<String, String> tags { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public SimplifiedNic simplifiedNicDetails { get; set; }
    }

    public class ComputeVms
    {
        public List<ComputeVm> value { get; set; }
    }
}