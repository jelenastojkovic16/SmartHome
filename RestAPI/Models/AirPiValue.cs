using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Models
{
    [Measurement("airpidata")]
    public class AirPiValue
    {
        [Column(IsTimestamp = true)]
        public DateTime Time { get; set; }

        [Column("airquality")]
        public double AirQuality { get; set; }

        [Column("carbonmonoxide")]
        public double CarbonMonoxide { get; set; }

        [Column("lightlevel")]
        public double LightLevel { get; set; }

        [Column("nitrogendioxide")]
        public int NitrogenDioxide { get; set; }

        [Column("pressure")]
        public int Pressure { get; set; }

        [Column("temperatureDHT")]
        public int Temperature  { get; set; }

        [Column("volume")]
        public int Volume { get; set; }
    }
}
