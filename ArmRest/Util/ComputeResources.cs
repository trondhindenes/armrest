using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ArmRest.Models;
using ArmRest.Util;
using Newtonsoft.Json;
using System.Net;
using System.Configuration;

namespace ArmRest.Util
{
    public static class ComputeResources
    {
        private static string ResourceGroupFilter = ConfigurationManager.AppSettings["Ansible:ResourceGroupNameFilter"];
        private static string SubscriptionFilter = ConfigurationManager.AppSettings["Ansible:SubscriptionFilter"];
        public static Dictionary<String,Object> GetHosts(String accessToken, String subscriptionId = null )
        {

            //get the subs
            List<String> subList = new List<string>();
            if (subscriptionId != null)
            {
                subList.Add(subscriptionId);
            }
            else
            {
                Subscriptions subscriptions = ListSubscriptions.GetSubscriptions();
                foreach (var sub in subscriptions.value)
                {
                    if (sub.id.Contains(SubscriptionFilter))
                    {
                        subList.Add(sub.subscriptionId);
                    }

                }

            }

            var ansibleHostList = new Dictionary<String, Object>();
            var ansibleHostVarsList = new Dictionary<String, Object>();
            

            accessToken = Adal.AccessToken();
            string authToken = "Bearer" + " " + accessToken;
            var client = new WebClient();
            client.Headers.Add("Authorization", authToken);
            client.Headers.Add("Content-Type", "application/json");

            foreach (var subId in subList)
            {

                Uri resourceGroupsUri = new Uri(String.Format("https://management.azure.com/subscriptions/{0}/resourcegroups?api-version=2015-01-01", subId));
                String text = "";


                text = client.DownloadString(resourceGroupsUri);
                ResourceGroups rgList = Newtonsoft.Json.JsonConvert.DeserializeObject<ResourceGroups>(text);
                List<ResourceGroup> filteredResourceGroups = new List<ResourceGroup>();
                foreach (var rg in rgList.value)
                {
                    if (rg.name.Contains(ResourceGroupFilter))
                    {
                        filteredResourceGroups.Add(rg);
                    }
                }


                foreach (ResourceGroup rg in filteredResourceGroups)
                {
                    //get each host
                    String rgId = rg.id;
                    String rgName = rg.name;
                    String rgComputeUrl = String.Format("https://management.azure.com{0}/providers/Microsoft.Compute/virtualmachines?api-version=2015-05-01-preview", rgId);
                    String rgVmsText = client.DownloadString(rgComputeUrl);
                    ComputeVms rgCompVms = JsonConvert.DeserializeObject<ComputeVms>(rgVmsText);
                    if (rgCompVms.value.Count != 0)
                    {
                        //resource group has vms. Fill the thing
                        List<Object> hostList = new List<Object>();
                        foreach (var vm in rgCompVms.value)
                        {
                            string vmname = vm.name;

                            if ((vm.tags != null) && (vm.tags.ContainsKey("AnsibleDomainSuffix")))
                            {
                                vmname = vm.name + "." + vm.tags["AnsibleDomainSuffix"];
                            }
                            else if ((rg.tags != null) && (rg.tags.ContainsKey("AnsibleDomainSuffix")))
                            {
                                vmname = vm.name + "." + rg.tags["AnsibleDomainSuffix"];
                            }

                            hostList.Add(vmname);

                            //check if we have hostsvars to add to meta
                            if ((vm.tags != null) && (vm.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")).Count() > 0))
                            {
                                var tagDict = new Dictionary<String, Object>();
                                foreach (var tag in vm.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")))
                                {
                                    tagDict.Add(tag.Key.ToLower().Replace("ansible__", ""), tag.Value);
                                }

                                ansibleHostVarsList.Add(vmname, tagDict);
                            }


                        }
                        //Add the rg to the main obj
                        var rgValueList = new Dictionary<String, Object>();
                        rgValueList.Add("hosts", hostList);

                        //add vars to the main object if they exists
                        if ((rg.tags != null) && (rg.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")).Count() > 0))
                        {
                            var tagDict = new Dictionary<String, Object>();
                            foreach (var tag in rg.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")))
                            {
                                tagDict.Add(tag.Key.ToLower().Replace("ansible__", ""), tag.Value);
                            }
                            rgValueList.Add("vars", tagDict);
                        }


                        ansibleHostList.Add(rgName, rgValueList);
                        var metaDict = new Dictionary<String, Object>();
                        metaDict.Add("hostvars", ansibleHostVarsList);
                        ansibleHostList.Add("_meta", metaDict);
                        
                    }
                }

            }

            return ansibleHostList;
        }

        public static ResourceGroup GetHostResourceGroup(String accessToken, ComputeVm thisVm)
        {
            string vmId = thisVm.id;
            var SplitUrl = vmId.Split('/');
            var rgName = SplitUrl[4];
            var subscriptionName = SplitUrl[2];

            string authToken = "Bearer" + " " + accessToken;
            var client = new WebClient();
            client.Headers.Add("Authorization", authToken);
            client.Headers.Add("Content-Type", "application/json");

            string singleRgUrl = String.Format("https://management.azure.com/subscriptions/{0}/resourcegroups/{1}?api-version=2015-01-01 ", subscriptionName, rgName);

            String text = "";
            
            text = client.DownloadString(singleRgUrl);
            ResourceGroup thisRg = Newtonsoft.Json.JsonConvert.DeserializeObject<ResourceGroup>(text);
            return thisRg;
        }
    }

    
}