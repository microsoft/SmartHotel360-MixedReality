using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartHotel.MixedReality.Api.Auth;
using SmartHotel.MixedReality.Api.Data;
using SmartHotel.MixedReality.Api.Topology;

namespace SmartHotel.MixedReality.Api.SensorData
{
    [Route("v1/desireddata")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class DesiredDataController: ControllerBase
    {
        private readonly IDatabaseHandler<DesiredData> _database;

        public DesiredDataController(IDatabaseHandler<DesiredData> database)
        {
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery(Name = "sensorIds")]string[] sensorIds)
        {
            if (sensorIds == null || sensorIds.Length == 0)
                return NotFound();
                        
            List<DesiredData> desiredData = await _database.FindIn(f => f.SensorId, sensorIds);

            return Ok(desiredData);
        }        
    }
}