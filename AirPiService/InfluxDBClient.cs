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

namespace AirPiService
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
                var value = JsonSerializer.Deserialize<AirPiValue>(payload);
                DateTime timestamp = DateTime.UtcNow;

                var point = PointData
                    .Measurement("airpidata")
                    .Tag("timestamp", timestamp.ToString())
                    .Field("datetime", value.DateTime.ToString())
                    .Field("volume", value.volume.ToString())
                    .Field("lightlevel", value.lightLevel.ToString())
                    .Field("temperatureDHT", value.temperature.ToString())
                    .Field("pressure", value.pressure.ToString())
                    //.Field("relativehumidity", value.Relative_Humidity.ToString())
                    .Field("airquality", value.airQuality.ToString())
                    .Field("carbonmonoxide", value.carbonMonoxide.ToString())
                    .Field("nitrogendioxide", value.nitrogenDioxide.ToString())
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                Console.WriteLine("Write in db check!");
                //await _client.GetWriteApiAsync().WritePointAsync(point, "airPi", "diplomskirad");
                Console.WriteLine("Write in db done!");

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Failed to write point:{ex.Message}");
            }
           
                

            }
    }

    public class AirPiValue
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        public double volume { get; set; }

        public double lightLevel { get; set; }

        public double temperature { get; set; }

        public double pressure { get; set; }

   

        public double Relative_Humidity { get; set; }

        public double airQuality { get; set; }

        public double carbonMonoxide { get; set; }

        public double nitrogenDioxide { get; set; }
    }
}
