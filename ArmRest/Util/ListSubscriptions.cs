using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

using ArmRest.Models;

namespace ArmRest.Util
{
    public class ListSubscriptions
    {
        public static Subscriptions GetSubscriptions()
        {
            String accessToken = "";
            Uri Url = new Uri("https://management.azure.com/subscriptions?&api-version=2015-01-01");
            String text = "";
            try
            {
                accessToken = Adal.AccessToken();
                string authToken = "Bearer" + " " + accessToken;
                //HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);

                //httpWebRequest.Headers.Add("Authorization", authToken);
                //var response = httpWebRequest.GetResponse();

                var client = new WebClient();
                client.Headers.Add("Authorization", authToken);
                client.Headers.Add("Content-Type", "application/json");
                text = client.DownloadString(Url);
                var subscriptions = JsonConvert.DeserializeObject<Subscriptions>(text);
                return subscriptions;

            }
            catch
            {
                return null;
            }
        }
    }
}