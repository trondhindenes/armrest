using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;

using ArmRest.Models;
using ArmRest.Util;

namespace ArmRest.Controllers
{
    public class TestAuthController : ApiController
    {
        public Subscriptions Get()
        {
            try
            {
                var subscriptions = ListSubscriptions.GetSubscriptions();
                return subscriptions;
            }
            catch
            {
                return null;
            }
            


        }

    }
}
