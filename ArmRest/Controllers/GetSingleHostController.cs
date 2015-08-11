using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using Newtonsoft.Json;

using ArmRest.Util;
using ArmRest.Models;
using System.Text;

namespace ArmRest.Controllers
{
    public class GetSingleHostController : ApiController
    {

        private static string SubscriptionFilter = ConfigurationManager.AppSettings["Ansible:SubscriptionFilter"];

        public HttpResponseMessage Get(String hostName)
        {
            List<String> subList = new List<string>();

            Subscriptions subscriptions = ListSubscriptions.GetSubscriptions();
            foreach (var sub in subscriptions.value)
            {
                if (sub.id.Contains(SubscriptionFilter))
                {
                    subList.Add(sub.subscriptionId);
                }
            }

            String accessToken = "";

            accessToken = Adal.AccessToken();
            string authToken = "Bearer" + " " + accessToken;
            var client = new WebClient();
            client.Headers.Add("Authorization", authToken);
            client.Headers.Add("Content-Type", "application/json");
            ComputeVms masterComputeVmsList = new ComputeVms();
            masterComputeVmsList.value = new List<ComputeVm>();

            foreach (var subId in subList)
            {
                Uri resourceGroupsUri = new Uri(String.Format("https://management.azure.com/subscriptions/{0}/providers/Microsoft.Compute/virtualmachines?api-version=2015-05-01-preview", subId));
                String text = "";
                text = client.DownloadString(resourceGroupsUri);
                ComputeVms computeVmsList = JsonConvert.DeserializeObject<ComputeVms>(text);
                foreach (var vm in computeVmsList.value)
                {
                    masterComputeVmsList.value.Add(vm);
                }
            }

            foreach (var vm in masterComputeVmsList.value)
            {
                ResourceGroup thisRg = ComputeResources.GetHostResourceGroup(accessToken, vm);
                if ((vm.tags != null) && (vm.tags.ContainsKey("AnsibleDomainSuffix")))
                {
                    vm.name = vm.name + "." + vm.tags["AnsibleDomainSuffix"];
                }
                else if ((thisRg.tags != null) && (thisRg.tags.ContainsKey("AnsibleDomainSuffix")))
                {
                    vm.name = vm.name + "." + thisRg.tags["AnsibleDomainSuffix"];
                }
            
            }
            
            ComputeVm thisComputeVm = masterComputeVmsList.value.Where(t => t.name == hostName).FirstOrDefault();
           


            Dictionary<String, String> TagsDict = new Dictionary<String, String>();
            foreach (var tag in thisComputeVm.tags.Where(t => t.Key.ToLower().StartsWith("ansible__")))
            {
                TagsDict.Add((tag.Key.ToLower().Replace("ansible__","")), tag.Value);
            }

            String json = JsonConvert.SerializeObject(TagsDict);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
