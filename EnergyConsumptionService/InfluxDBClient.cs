using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnergyConsumptionService
{
    public interface IDbClient
    {
        Task WriteToDatabase(string payload);
    }
    public class InfluxDbClient : IDbClient
    {
        public readonly InfluxDBClient _client;
        public InfluxDbClient()
        {
           _client = InfluxDBClientFactory.Create(url: "http://localhost:8086", "admin", "admin2023".ToCharArray());
        }
        public async Task WriteToDatabase(string payload)
        {
            try
            {
                var value = JsonSerializer.Deserialize<EnergyConsumptionValue>(payload);
                DateTime timestamp = DateTime.UtcNow;

                var point = PointData
                    .Measurement("energyConsumptionData")
                    .Field("furnace", value.furnace.ToString())
                    .Field("dishwasher", value.dishwasher.ToString())
                    .Field("homeOffice", value.homeOffice.ToString())
                    .Field("houseOverall", value.houseOverall.ToString())
                    .Field("fridge", value.fridge.ToString())
                    .Field("garageDoor", value.garageDoor.ToString())
                    .Field("kitchen", value.kitchen.ToString())
                    .Field("barn", value.barn.ToString())
                    .Field("livingRoom", value.livingRoom.ToString())
                    .Field("microwave", value.microwave.ToString())
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                Console.WriteLine("Write in db check!");
                //await _client.GetWriteApiAsync().WritePointAsync(point, "energyConsumption", "diplomskirad");
                Console.WriteLine("Write in db done!");

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Failed to write point:{ex.Message}");
            }
           
                

            }
    }

    public class EnergyConsumptionValue
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public double furnace { get; set; }

        public double houseOverall { get; set; }

        public double dishwasher { get; set; }

        public double homeOffice { get; set; }

        public double fridge { get; set; }

        public double garageDoor { get; set; }

        public double kitchen { get; set; }

        public double barn { get; set; }

        public double livingRoom { get; set; }

        public double microwave { get; set; }

    }
}
