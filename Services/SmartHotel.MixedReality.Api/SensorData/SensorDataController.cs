using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartHotel.MixedReality.Api.Anchors;
using SmartHotel.MixedReality.Api.Auth;
using SmartHotel.MixedReality.Api.Data;

namespace SmartHotel.MixedReality.Api.SensorData
{

    [Route("v1/sensordata")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class SensorDataController : ControllerBase
    {
        private readonly IDatabaseHandler<SensorData> _database;
        private DatabaseSettings _config;
        private MongoClient _documentClient;

        public SensorDataController(IOptions<DatabaseSettings> config, IDatabaseHandler<SensorData> database)
        {
            _database = database;
            _config = config.Value;
            _documentClient = new MongoClient(_config.MongoDbConnectionString);
        }

        [HttpGet]
        public IActionResult Get([FromQuery(Name = "roomIds")]string[] roomIds)
        {
            if (roomIds == null || roomIds.Length == 0)
                return NotFound();

            List<SensorData> sensorData = new List<SensorData>();

            try
            {
                IMongoDatabase db = _documentClient.GetDatabase(_config.MongoDbName);
                IMongoCollection<SensorData> sensorDataTable = db.GetCollection<SensorData>(nameof(SensorData));

                string filter = $"{{roomId: {{'$in': [{ToMongoArrayString(roomIds)}]}}}}";
                Console.WriteLine(filter);

                List<SensorData> results = sensorDataTable.Find(filter).ToList();
                if (results != null && results.Count > 0)
                {
                    Dictionary<string, SensorData[]> sensorDatasByRoom = results.GroupBy(sd => sd.RoomId)
                        .ToDictionary(g => g.Key, g => g.ToArray());
                    foreach (KeyValuePair<string, SensorData[]> kvp in sensorDatasByRoom)
                    {
                        SensorData[] latestSensorDatas = kvp.Value
                            .GroupBy(sd => sd.SensorDataType)
                            .Select(g => g.OrderByDescending(sd => sd.EventTimestamp).First()).ToArray();

                        sensorData.AddRange(latestSensorDatas);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            return Ok(sensorData);
        }



        private string ToMongoArrayString(string[] arr)
        {
            string result = string.Empty;

            for (int i = 0; i < arr.Length; i++)
            {
                result += $"\"{arr[i]}\"";

                if (i < arr.Length - 1)
                    result += ",";
            }

            return result;
        }
    }
}