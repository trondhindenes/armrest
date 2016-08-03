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
        private static string ReturnOnlyPoweredOnVms = ConfigurationManager.AppSettings["Ansible:ReturnRunningVmsOnly"];
        private static string HostCasingSetting = ConfigurationManager.AppSettings["Ansible:HostCasing"];
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
                    if (ResourceGroupFilter == "*")
                    {
                        filteredResourceGroups.Add(rg);
                    }
                    else if (rg.name.ToLower().Contains(ResourceGroupFilter.ToLower()))
                    {
                        filteredResourceGroups.Add(rg);
                    }
                }


                foreach (ResourceGroup rg in filteredResourceGroups)
                {
                    //get each host
                    String rgId = rg.id;
                    String rgName = rg.name;

                    //remove Prod/Test/Dev from rg name
                    if (rgName.ToLower().StartsWith("prod."))
                    {
                        rgName = rgName.Remove(0, 5);
                    }
                    else if (rgName.ToLower().StartsWith("test."))
                    {
                        rgName = rgName.Remove(0, 5);
                    }
                    else if (rgName.ToLower().StartsWith("dev."))
                    {
                        rgName = rgName.Remove(0, 4);
                    }

                    
                    

                    String rgComputeUrl = String.Format("https://management.azure.com{0}/providers/Microsoft.Compute/virtualmachines?api-version=2015-05-01-preview", rgId);
                    String rgVmsText = client.DownloadString(rgComputeUrl);
                    ComputeVms rgCompVms = JsonConvert.DeserializeObject<ComputeVms>(rgVmsText);
                    if (rgCompVms.value.Count != 0)
                    {
                        //resource group has vms. Fill the thing
                        List<Object> hostList = new List<Object>();
                        foreach (var vm in rgCompVms.value)
                        {
                            var tagDict = new Dictionary<String, Object>();
                            bool VerifyPoweredOnVM = true;
                            if (ReturnOnlyPoweredOnVms.ToLower() == "true")
                            {
                                VerifyPoweredOnVM = GetVmPowerStatus(accessToken, vm);
                            }

                            var simplifiedNic = GetNicDetails(accessToken, vm);
                            vm.simplifiedNicDetails = simplifiedNic;


                            if (HostCasingSetting == "UpperCase")
                            {

                                vm.name = vm.name.ToUpper();
                            }

                            if (HostCasingSetting == "LowerCase")
                            {
                                vm.name = vm.name.ToLower();
                            }

                            string vmname = vm.name;



                            if ((vm.tags != null) && (vm.tags.ContainsKey("AnsibleDomainSuffix")))
                            {
                                vmname = vm.name + "." + vm.tags["AnsibleDomainSuffix"];
                            }
                            else if ((rg.tags != null) && (rg.tags.ContainsKey("AnsibleDomainSuffix")))
                            {
                                vmname = vm.name + "." + rg.tags["AnsibleDomainSuffix"];
                            }
                            String ansibleReturnType = null;
                            if ((vm.tags != null) && (vm.tags.ContainsKey("AnsibleReturn")))
                            {
                                ansibleReturnType = vm.tags["AnsibleReturn"];
                            }
                            else if ((rg.tags != null) && (rg.tags.ContainsKey("AnsibleReturn")))
                            {
                                ansibleReturnType = rg.tags["AnsibleReturn"];
                            }

                            //If ansiblereturn is set, figure out what to return
                            if (ansibleReturnType != null)
                            {
                                if ((ansibleReturnType.ToLower() == "privateipaddress") && (vm.simplifiedNicDetails.InternalIpAddress != null))
                                {
                                    vmname = vm.simplifiedNicDetails.InternalIpAddress;
                                }
                                else if ((ansibleReturnType.ToLower() == "publicipaddress") && (vm.simplifiedNicDetails.PublicIpAddress != null))
                                {
                                    vmname = vm.simplifiedNicDetails.PublicIpAddress;
                                }
                                else if ((ansibleReturnType.ToLower() == "publichostname") && (vm.simplifiedNicDetails.PublicHostName != null))
                                {
                                    vmname = vm.simplifiedNicDetails.PublicHostName;
                                }
                                else if ((ansibleReturnType.ToLower() == "privateipaddress_asansiblehost") && (vm.simplifiedNicDetails.InternalIpAddress != null))
                                {

                                    tagDict.Add("ansible_host", vm.simplifiedNicDetails.InternalIpAddress);
                                }
                                else if ((ansibleReturnType.ToLower() == "publicipaddress_asansiblehost") && (vm.simplifiedNicDetails.PublicIpAddress != null))
                                {
                                    tagDict.Add("ansible_host", vmname = vm.simplifiedNicDetails.PublicIpAddress);
                                }
                                else if ((ansibleReturnType.ToLower() == "publichostname_asansiblehost") && (vm.simplifiedNicDetails.PublicHostName != null))
                                {
                                    tagDict.Add("ansible_host", vmname = vmname = vm.simplifiedNicDetails.PublicHostName);
                                }


                            }

                            //vmname is now either computername, computername+domain, one ip address or public fqdn


                            if (VerifyPoweredOnVM == true)
                            {
                                hostList.Add(vmname);
                                //check if we have hostsvars to add to meta
                                if ((vm.tags != null) && (vm.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")).Count() > 0))
                                {
                                    foreach (var tag in vm.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")))
                                    {
                                        tagDict.Add(tag.Key.ToLower().Replace("ansible__", ""), tag.Value);
                                    }


                                }
                            }

                            if (tagDict.Count > 0)
                            {
                                //add tags if any
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
                        
                        
                    }
                }

            }

            //Add the _meta thing to the result output
            var metaDict = new Dictionary<String, Object>();
            metaDict.Add("hostvars", ansibleHostVarsList);
            ansibleHostList.Add("_meta", metaDict);
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

        public static bool GetVmPowerStatus(String accessToken, ComputeVm thisVm)
        {
            bool returnResult = false;

            string vmId = thisVm.id;
            var SplitUrl = vmId.Split('/');
            var rgName = SplitUrl[4];
            var subscriptionName = SplitUrl[2];

            string authToken = "Bearer" + " " + accessToken;
            var client = new WebClient();
            client.Headers.Add("Authorization", authToken);
            client.Headers.Add("Content-Type", "application/json");

            string vmStatusUrl = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Compute/virtualMachines/{2}?$expand=instanceView&api-version=2015-06-15", subscriptionName, rgName, thisVm.name);
            String text = "";
            text = client.DownloadString(vmStatusUrl);
            var vmStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<ComputeVm>(text);
            var InstanceView = vmStatus.properties.instanceView.statuses;
            var ThisInstanceviewPowerState = InstanceView.Where(p => p.code.Contains("PowerState")).FirstOrDefault();
            var ThisInstanceviewProvisioningState = InstanceView.Where(p => p.code.Contains("ProvisioningState")).FirstOrDefault();
            if ((ThisInstanceviewPowerState.code == "PowerState/running") && (ThisInstanceviewProvisioningState.code == "ProvisioningState/succeeded"))
            {
                
                returnResult = true;
            }
            
            System.Diagnostics.Debug.WriteLine(string.Format("VM {0} in RG {1} has power state {2}", thisVm.name, rgName, ThisInstanceviewPowerState.code));
            return returnResult;            
        }

        public static SimplifiedNic GetNicDetails(String accessToken, ComputeVm Vm)
        {
            string authToken = "Bearer" + " " + accessToken;
            var client = new WebClient();
            client.Headers.Add("Authorization", authToken);
            client.Headers.Add("Content-Type", "application/json");

            var nic = Vm.properties.networkProfile.networkInterfaces.FirstOrDefault();
            string nicLink = nic.id;
            String nicUrl = String.Format("https://management.azure.com{0}{1}", nicLink, "?api-version=2015-05-01-preview");
            String nicText = client.DownloadString(nicUrl);
            //ComputeVms rgCompVms = JsonConvert.DeserializeObject<ComputeVms>(rgVmsText);
            ArmRest.Models.NetworkInterfaceDetails.RootObject nicObj = JsonConvert.DeserializeObject<ArmRest.Models.NetworkInterfaceDetails.RootObject>(nicText);

            String InternalIpAddress = nicObj.properties.ipConfigurations.FirstOrDefault().properties.privateIPAddress;

            String PublicIpLink = nicObj.properties.ipConfigurations.FirstOrDefault().properties.publicIPAddress.id;
            String publicIpUrl = String.Format("https://management.azure.com{0}{1}", PublicIpLink, "?api-version=2015-05-01-preview");
            String publicIpText = client.DownloadString(publicIpUrl);

            ArmRest.Models.PublicIpAddress.RootObject publicIpAddressObj = JsonConvert.DeserializeObject<ArmRest.Models.PublicIpAddress.RootObject>(publicIpText);

            SimplifiedNic thisSimplifiedNic = new SimplifiedNic();
            thisSimplifiedNic.InternalIpAddress = InternalIpAddress;
            

            String PublicIpAddress = null;
            try
            {
                PublicIpAddress = publicIpAddressObj.properties.ipAddress;
                thisSimplifiedNic.PublicIpAddress = PublicIpAddress;
            }
            catch
            { }

            try
            {
                thisSimplifiedNic.PublicHostName = publicIpAddressObj.properties.dnsSettings.fqdn;
            }
            catch
            { }
            

            

            return thisSimplifiedNic;

        }
    }

    
}