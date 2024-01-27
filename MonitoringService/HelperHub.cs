using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace MonitoringService
{
    public class HelperHub : Hub
    {
        public HelperHub() { }

        public async Task SendMessage(string message)
        {
            JObject jsonObject = JObject.Parse(message);
            await Clients.All.SendAsync("alert",jsonObject);
        }
    }
}
