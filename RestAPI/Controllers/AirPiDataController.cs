using InfluxDB.Client;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Models;
using System;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AirPiDataController : ControllerBase
    {
        private readonly InfluxDBClient _idbClient;
        private readonly string influxdbOrganization = "diplomskirad";
        private readonly string influxdbBucket = "airPi";

        public AirPiDataController(InfluxDBClient idbClient)
        {
            _idbClient = idbClient;
        }

        [Route("getAirPi")]
        [HttpGet]
        public async Task<ActionResult> GetAirPiData()
        {
            try
            {
                var iqlQuery = "from(bucket: \"airPi\")" +
                               "|> range(start: 0)" +
                               "|> filter(fn: (r) => r[\"_measurement\"] == \"airpidata\")" +
                               "|> filter(fn: (r) => r[\"_field\"] == \"airquality\" or r[\"_field\"] == \"carbonmonoxide\" or r[\"_field\"] == \"datetime\" or r[\"_field\"] == \"lightlevel\" or r[\"_field\"] == \"nitrogendioxide\" or r[\"_field\"] == \"temperatureDHT\" or r[\"_field\"] == \"pressure\" or r[\"_field\"] == \"volume\")\r\n" +
                               "|> pivot(rowKey: [\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

                var result = await _idbClient.GetQueryApi().QueryAsync<AirPiValue>(iqlQuery, influxdbOrganization);
                Random random = new Random();
                int number = 26;
                int i = 0;



                var result2 = result.Select(item => {
                    if(i % 3 == 0)
                    {
                        number--;
                    }
                    if(i == 3 && i == 6)
                    {
                        number+=2;  
                    }
                    item.Temperature = number;
                    i++;
                    return item;
                }).ToList();
             
                return Ok(result2.OrderBy(r => r.Time));

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return BadRequest(ex.Message);
            }
           
        }

        [Route("getEnergy")]
        [HttpGet]
        public async Task<IActionResult> GetEnergyConsumptionData()
        {
            try
            {
                var iqlQuery = "from(bucket: \"energyConsumption\")" +
                               "|> range(start: 0)" +
                               "|> filter(fn: (r) => r[\"_measurement\"] == \"energyConsumptionData\")" +
                               "|> filter(fn: (r) => r[\"_field\"] == \"barn\" or r[\"_field\"] == \"dishwasher\" or r[\"_field\"] == \"fridge\" or r[\"_field\"] == \"furnace\" or r[\"_field\"] == \"garageDoor\" or r[\"_field\"] == \"homeOffice\" or r[\"_field\"] == \"houseOverall\" or r[\"_field\"] == \"kitchen\" or r[\"_field\"] == \"livingRoom\" or r[\"_field\"] == \"microwave\")" +
                               "|> pivot(rowKey: [\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

                var result = await _idbClient.GetQueryApi().QueryAsync<EnergyConsumptionValue>(iqlQuery, influxdbOrganization);

           

                return Ok(result.OrderBy(r => r.Time));

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                return BadRequest(ex.Message);
            }
        }

    }

}
