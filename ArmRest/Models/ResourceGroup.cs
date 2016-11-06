using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArmRest.Models
{

    public class ResourceGroup
    {
        public string id { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public ResourceGroupProperties properties { get; set; }
        public Dictionary<String, String> tags { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public class ResourceGroupProperties
    {
        public string provisioningState { get; set; }
    }

  
    public class ResourceGroups
    {
        public List<ResourceGroup> value { get; set; }
    }
}