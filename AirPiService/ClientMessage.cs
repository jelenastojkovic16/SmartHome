using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;

namespace MonitoringService
{
    public class ClientMessage
    {
        private  IHubContext<HelperHub> _hub { get; set; }
        public ClientMessage(IHubContext<HelperHub> hub) 
        { 
            _hub = hub;
        }
        public async Task SendMessage(string message)
        {
            try
            {
              
              
                await _hub.Clients.All.SendAsync("updateData", message );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            
        }
    }
}
