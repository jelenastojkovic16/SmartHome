using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Adapter;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.SignalR;
using MonitoringService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEmqxClient, EmqxClient>()
                .AddSingleton<ClientMessage>();
builder.Services.AddSignalR();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
     builder.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials();
}));

var app = builder.Build();
app.UseRouting();
app.MapHub<HelperHub>("/MessageHub");
app.UseCors("CorsPolicy");

var emqxClient = app.Services.GetRequiredService<IEmqxClient>();
var clientMessage = app.Services.GetRequiredService<ClientMessage>();
//var dbClient = serviceProvider.GetRequiredService<IDbClient>();

// topics
const string eKuiperTopic = "eKuiper/AirQuality";
const string eKuiperTopicFire = "eKuiper/Fire";
const string airService = "airService";
const string energyService = "energyService";

bool isTemperatureConditionMet = false;
bool isEnergyConditionMet = false;
bool isLightLevlConditionMet = false;
bool isHouseOverallConditionMet = false;


//conection
await emqxClient.ConnectAsync("172.26.176.1", "your_client_id");
await emqxClient.SubscribeAsync(new List<string> { eKuiperTopic, eKuiperTopicFire, airService, energyService });

emqxClient.ConfigureMessageHandler(MessageReceivedFunctionAsync);

Task MessageReceivedFunctionAsync(MqttApplicationMessageReceivedEventArgs args)
{
    string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
    var data = (JToken)(JObject)JsonConvert.DeserializeObject(payload);
    string topic = args.ApplicationMessage.Topic;
    if (topic == airService)
    {
        var temperature = data.SelectToken("temperature")?.Value<Int64>();
        var lightLevel = data.SelectToken("lightLevel")?.Value<Int64>();
        if (temperature != null)
        {
            isTemperatureConditionMet = temperature >= 25;
        }

        if (lightLevel != null)
        {
            isLightLevlConditionMet = lightLevel >= 450;
        }

    }
    else if (topic == energyService)
    {
        var furnace = data.SelectToken("furnace")?.Value<Double>();
        var houseOverall = data.SelectToken("houseOverall")?.Value<Double>();

        if (furnace != null)
        {
            isEnergyConditionMet = furnace >= 3;
        }

        if (houseOverall != null)
        {
            isHouseOverallConditionMet = houseOverall >= 10;
        }
    }
    else
    {
        switch (topic)
        {
            case eKuiperTopicFire:
                Console.WriteLine("Possible fire detection!");
                clientMessage.SendMessage("Possible fire detection!");

                break;
            case eKuiperTopic:
                Console.WriteLine("High toxic gas concentracion!");
                clientMessage.SendMessage("High toxic gas concentracion!");
                break;
            default:
                Console.WriteLine("");
                break;
        }
    }


    if (isTemperatureConditionMet && isEnergyConditionMet)
    {
        Console.WriteLine("Possible energy saving.Turn off the furnace");
        clientMessage.SendMessage("Possible energy saving.Turn off the furnace!");
        isEnergyConditionMet = isEnergyConditionMet = false;
    }
    if (isHouseOverallConditionMet && isLightLevlConditionMet)
    {
        Console.WriteLine("Possible energy saving. Reduce the light");
        clientMessage.SendMessage("Possible energy saving. Reduce the light");
        isLightLevlConditionMet = isHouseOverallConditionMet = false;
    }

    return Task.CompletedTask;
}


app.MapGet("/", () => "Hello World!");

app.Run();
