using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using SmartHotel.MixedReality.Api.Auth;
using SmartHotel.MixedReality.Api.Data;

namespace SmartHotel.MixedReality.Api.SharedState
{
    [Route("v1/sharedstate")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class SharedStateController
    {
        private readonly IDatabaseHandler<SharedState> _database;
        private readonly IClock _clock;

        public SharedStateController(IDatabaseHandler<SharedState> database, IClock clock)
        {
            _database = database;
            _clock = clock;
        }

        [Route("{anchorSetId}")]
        [HttpGet]
        public async Task<SharedState> GetSharedState([FromRoute]string anchorSetId)
        {
            SharedState sharedState = await _database.FindOne(a => a.Id == anchorSetId);
            return sharedState;
        }

        [Route("{anchorSetId}")]
        [HttpPut]
        public async Task UpdateSharedState([FromRoute]string anchorSetId, [FromBody]SharedStateDto newState)
        {
            SharedState existingSharedState = await GetSharedState(anchorSetId) ?? new SharedState()
            {
                Id = anchorSetId, 
                CreatedAt = _clock.GetCurrentInstant(),
                ToggledSensorPanels = new Dictionary<string, bool>()
            };
            existingSharedState.CurrentSelectedSpace = newState.CurrentSelectedSpace;
            existingSharedState.UpdatedAt = _clock.GetCurrentInstant();
            existingSharedState.ToggledSensorPanels = newState.ToggledSensorPanels;
            await _database.ReplaceOneAsync(s => s.Id == anchorSetId, existingSharedState);
        }

        
        [Route("{anchorSetId}/device/{deviceId}/{toggled}")]
        [HttpPut]
        public async Task<StatusCodeResult> TogglePanelForDevice([FromRoute]string anchorSetId, [FromRoute]string deviceId, [FromRoute]bool toggled)
        {
            SharedState existingSharedState = await GetSharedState(anchorSetId);
            if(existingSharedState == null)
                return new NotFoundResult();
            throw new NotImplementedException();
        }
    }

    public class SharedStateDto
    {
        public SharedStateDto()
        {
            
        }
        public SharedStateDto(SharedState sharedState)
        {
            CurrentSelectedSpace = sharedState.CurrentSelectedSpace;
        }

        public string CurrentSelectedSpace { get; set; }
        public Dictionary<string, bool> ToggledSensorPanels { get; set; }
    }
}
