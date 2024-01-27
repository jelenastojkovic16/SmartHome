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
using MonitoringService;
using AirPiService;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IEmqxClient, EmqxClient>()
                .AddSingleton<ClientMessage>()
                .AddSingleton<IDbClient, InfluxDbClient>();

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

// clients
var emqxClient = app.Services.GetRequiredService<IEmqxClient>();
var clientMessage = app.Services.GetRequiredService<ClientMessage>();
var dbClient = app.Services.GetRequiredService<IDbClient>();

//topics
string edgeXTopic = "edgex/sensor_value";
string eKuiperTopic = "analytics/airpivalues";
string airServiceTopic = "airService";



// conection
await emqxClient.ConnectAsync("172.26.176.1", "your_client_id");
await emqxClient.SubscribeAsync(new List<string> { edgeXTopic });

emqxClient.ConfigureMessageHandler(MessageReceivedFunctionAsync);

Task MessageReceivedFunctionAsync(MqttApplicationMessageReceivedEventArgs args)
{
    string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
    if (args.ApplicationMessage.Topic == edgeXTopic)
    {
        var data = (JToken)(JObject)JsonConvert.DeserializeObject(payload);
        var device = data.SelectToken("device").Value<String>();

        if (device != "RaspberryPi Sensor")
            return null;
        //Console.WriteLine(payload.ToString());
        JObject jsonObject = ParseData(data);
        // Pretvaranje JSON objekta u string
        string jsonResult = jsonObject.ToString();

        Console.WriteLine(jsonResult);

        emqxClient.PublishMessageAsync(eKuiperTopic, jsonResult);
        emqxClient.PublishMessageAsync(airServiceTopic, jsonResult);
        //emqxClient.PublishMessageAsync("eKuiper/Fire", jsonResult);
        clientMessage.SendMessage(jsonResult);

        //database
        dbClient.WriteToDatabase(jsonResult);


        return Task.CompletedTask;
    }

    return Task.CompletedTask;
}

JObject ParseData(JToken data)
{
    var id = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "id");
    var timestamp = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "timestamp");
    var volume = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "volume");
    var lightLevel = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "lightLevel");
    var temperature = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "temperaturedht");
    var pressure = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "pressure");
    var relativeHumidity = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "relativehumidity");
    var airQuality = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "airquality");
    var carbonMonoxide = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "carbonMonoxide");
    var nitrogenDioxide = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "nitrogenDioxide");

    var idValue = id?.SelectToken("value").Value<string>();
    var timestampValue = timestamp?.SelectToken("value").Value<string>();
    var volumeValue = volume?.SelectToken("value").Value<Int64>();
    var lightLevelValue = lightLevel?.SelectToken("value").Value<Int64>();
    var temperatureValue = temperature?.SelectToken("value").Value<Int64>();
    var pressureValue = pressure?.SelectToken("value").Value<Int64>();
    var relativeHumidityValue = relativeHumidity?.SelectToken("value").Value<Int64>();
    var airQualityValue = airQuality?.SelectToken("value").Value<Int64>();
    var carbonMonoxideValue = carbonMonoxide?.SelectToken("value").Value<Int64>();
    var nitrogenDioxideValue = nitrogenDioxide?.SelectToken("value").Value<Int64>();


    JObject jsonObject = new JObject
        {
            { "id", idValue },
            { "timestamp", timestampValue },
            { "volume", volumeValue },
            { "lightLevel", lightLevelValue },
            { "temperature", temperatureValue },
            { "pressure", pressureValue },
            { "relativeHumidity", relativeHumidityValue },
            { "airQuality", airQualityValue },
            { "carbonMonoxide", carbonMonoxideValue },
            { "nitrogenDioxide", nitrogenDioxideValue }
        };

    return jsonObject;

}

app.MapGet("/", () => "Hello World!");

app.Run();
