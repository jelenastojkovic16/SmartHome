using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Models
{
    [Measurement("energyConsumptionData")]
    public class EnergyConsumptionValue
    {
        [Column(IsTimestamp = true)]
        public DateTime Time { get; set; }

        [Column("barn")]
        public double Barn { get; set; }

        [Column("fridge")]
        public double Fridge { get; set; }

        [Column("dishwasher")]
        public double Dishwasher { get; set; }

        [Column("furnace")]
        public int Furnace { get; set; }

        [Column("garageDoor")]
        public int GarageDoor { get; set; }

        [Column("homeOffice")]
        public int HomeOffice { get; set; }

        [Column("houseOverall")]
        public int HouseOverall { get; set; }

        [Column("livingRoom")]
        public int LivingRoom { get; set; }

        [Column("microwave")]
        public int Microwave { get; set; }
    }
}
