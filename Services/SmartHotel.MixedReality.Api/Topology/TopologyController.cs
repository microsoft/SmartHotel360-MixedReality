using Microsoft.AspNetCore.Mvc;
using SmartHotel.MixedReality.Api.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHotel.MixedReality.Api.Topology
{
    [Route("v1/topology")]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class TopologyController
    {
        private readonly ITopologyClient _client;

        public TopologyController(ITopologyClient client)
        {
            _client = client;
        }

        // GET: v1/topology
        [HttpGet]
        public async Task<List<Space>> Get()
        {
            ICollection<Space> spaces = await _client.GetSpaces();

            return spaces.ToList();
        }

    }
}
