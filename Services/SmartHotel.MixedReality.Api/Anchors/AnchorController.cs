using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartHotel.MixedReality.Api.Auth;

namespace SmartHotel.MixedReality.Api.Anchors
{   
    [Route("v1/anchorsets")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class AnchorController : Controller
    {
        private readonly IAnchorSetService _anchorSetService;

        public AnchorController(IAnchorSetService anchorSetService)
        {
            _anchorSetService = anchorSetService;
        }

        [HttpGet]
        public async Task<List<AnchorSetResult>> GetAsync()
        {
            List<AnchorSet> results = await _anchorSetService.GetAllAnchorSets();

            return results.Select(a => new AnchorSetResult(a)).ToList();
        }

        [HttpGet]
        [Route("virtual/{anchorSetId}")]
        public async Task<AnchorSet> GetVirtualAnchorSet([FromRoute]string anchorSetId)
        {
            AnchorSet results = await _anchorSetService.GetVirtualAnchorSet(anchorSetId);
            return results;
        }

        [HttpGet]
        [Route("physical/{anchorSetId}")]
        public async Task<AnchorSet> GetPhysicalAnchorSet([FromRoute]string anchorSetId)
        {
            AnchorSet results = await _anchorSetService.GetPhysicalAnchorSet(anchorSetId);
            return results;
        }

        [HttpPost]
        public async Task<AnchorSet> CreateAnchorSet([FromBody]string anchorSetName)
        {
            AnchorSet anchorSet = await _anchorSetService.CreateAnchorSet(anchorSetName);
            return anchorSet;
        }

        [HttpPut]
        [Route("virtual/{anchorSetId}/{anchorId}")]
        public async Task<AnchorSet> CreateVirtualAnchor([FromRoute]string anchorSetId, [FromRoute]string anchorId)
        {
            AnchorSet anchorSet = await _anchorSetService.CreateVirtualAnchor(anchorSetId, anchorId);
            return anchorSet;
        }



        [HttpPut]
        [Route("physical/{anchorSetId}/{anchorId}")]
        public async Task<AnchorSet> CreatePhysicalAnchor([FromRoute]string anchorSetId, [FromRoute]string anchorId, [FromBody]string deviceId)
        {
            AnchorSet anchorSet = await _anchorSetService.CreatePhsyicalAnchor(anchorSetId, anchorId, deviceId);
            return anchorSet;
        }


        [HttpDelete]
        [Route("{anchorSetId}")]
        public async Task DeleteAnchorSet([FromRoute] string anchorSetId)
        {
            await _anchorSetService.DeleteAnchorSet(anchorSetId);
        }

        
        [HttpDelete]
        [Route("{anchorSetId}/{anchorId}")]
        public async Task<AnchorSet> DeleteAnchor([FromRoute] string anchorSetId, [FromRoute]string anchorId)
        {
            return await _anchorSetService.DeleteAnchor(anchorSetId, anchorId);
        }
    }
}