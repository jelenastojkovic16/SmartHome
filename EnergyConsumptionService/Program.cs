using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using EnergyConsumptionService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IEmqxClient, EmqxClient>()
                .AddSingleton<IDbClient, InfluxDbClient>();
var app = builder.Build();

var emqxClient = app.Services.GetRequiredService<IEmqxClient>();
var dbClient = app.Services.GetRequiredService<IDbClient>();

//topics
string edgeXTopic = "edgex/sensor_value";
string eKuiperTopic = "analytics/airpivalues";
string energyTopic = "energyService";


//conection
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

        if (device != "EnergyConsumptionDevice")
            return null;
        Console.WriteLine(payload.ToString());
        JObject jsonObject = ParseData(data);
        // Pretvaranje JSON objekta u string
        string jsonResult = jsonObject.ToString();

        Console.WriteLine(jsonResult);

        emqxClient.PublishMessageAsync(eKuiperTopic, jsonResult);
        emqxClient.PublishMessageAsync(energyTopic, jsonResult);

        //database
        dbClient.WriteToDatabase(jsonResult);
        return Task.CompletedTask;
    }
    return Task.CompletedTask;
}

JObject ParseData(JToken data)
{

    var timestamp = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "timestampEC");
    var houseOverall = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "houseOverall");
    var dishwasher = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "dishwasher");
    var furnace = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "furnace");
    var homeOffice = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "homeOffice");
    var fridge = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "fridge");
    var garageDoor = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "garageDoor");
    var kitchen = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "kitchen");
    var barn = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "barn");
    var microwave = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "microwave");
    var livingRoom = data.SelectToken("readings").FirstOrDefault(r => r.SelectToken("name").Value<string>() == "livingRoom");

    var timestampValue = timestamp?.SelectToken("value").Value<string>();
    var houseOverallValue = houseOverall?.SelectToken("value").Value<Double>();
    var dishwasherValue = dishwasher?.SelectToken("value").Value<Double>();
    var furnaceValue = furnace?.SelectToken("value").Value<Double>();
    var homeOfficeValue = homeOffice?.SelectToken("value").Value<Double>();
    var fridgeValue = fridge?.SelectToken("value").Value<Double>();
    var garageDoorValue = garageDoor?.SelectToken("value").Value<Double>();
    var kitchenValue = kitchen?.SelectToken("value").Value<Double>();
    var barnValue = barn?.SelectToken("value").Value<Double>();
    var microwaveValue = microwave?.SelectToken("value").Value<Double>();
    var livingRoomValue = livingRoom?.SelectToken("value").Value<Double>();


    JObject jsonObject = new JObject
        {
            { "timestamp", timestampValue },
            { "houseOverall", houseOverallValue },
            { "dishwasher", dishwasherValue },
            { "furnace", furnaceValue },
            { "homeOffice", homeOfficeValue },
            { "fridge", fridgeValue },
            { "garageDoor", garageDoorValue },
            { "kitchen", kitchenValue },
            { "barn", barnValue },
            { "microwave", microwaveValue },
            { "livingRoom", livingRoomValue },

        };

    return jsonObject;

}

app.MapGet("/", () => "Hello World!");

app.Run();
