using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Configuration;

using ArmRest.Util;
using ArmRest.Models;
using System.Dynamic;
using ClaySharp;

namespace ArmRest.Controllers
{
    public class ListHostsController : ApiController
    {
        private static string ResourceGroupFilter = ConfigurationManager.AppSettings["Ansible:ResourceGroupNameFilter"];
        private static string SubscriptionFilter = ConfigurationManager.AppSettings["Ansible:SubscriptionFilter"];
        public HttpResponseMessage Get(String subscriptionId = null, bool UseExternalIpAddress = false)
        {
            String accessToken = Adal.AccessToken();

            var ansibleHosts = ComputeResources.GetHosts(accessToken);
            
            String json = JsonConvert.SerializeObject(ansibleHosts);
            System.Diagnostics.Debug.WriteLine(json);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
            
        }

        
    }
}
