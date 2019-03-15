using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartHotel.MixedReality.Api.Auth;
using SmartHotel.MixedReality.Api.Topology;

namespace SmartHotel.MixedReality.Api.BrandImages
{
    [Route( "v1/brandimages" )]
    [ApiController]
    [ServiceFilter(typeof(AuthorizationFilterAttribute))]
    public class BrandImagesController : ControllerBase
    {
        private readonly ITopologyClient _topologyClient;
        private readonly IDigitalTwinsClient _digitalTwinsClient;

        public BrandImagesController(ITopologyClient topologyClient, IDigitalTwinsClient digitalTwinsClient)
        {
            _topologyClient = topologyClient;
            _digitalTwinsClient = digitalTwinsClient;
        }

        [HttpGet("{spaceId}")]
        [Produces("image/png")]
        public async Task<IActionResult> GetBrandImage(string spaceId)
        {
            ICollection<DigitalTwinsSpace> spaces = await _topologyClient.GetBrandLevelSpaces();
            DigitalTwinsSpace space = spaces.FirstOrDefault(s => s.id == spaceId);
            Property imagePath = space?.properties?.SingleOrDefault(p=> string.Equals(p.name, "ImagePath", StringComparison.InvariantCultureIgnoreCase));
            if(!Uri.TryCreate(imagePath?.value, UriKind.Absolute, out Uri uri))
            {
                return NotFound();
            }
            else
            {
                byte[] imageResult = await _digitalTwinsClient.GetFromDigitalTwinsAsBytes(uri.AbsoluteUri);
                return File(imageResult, "image/png");
            }
        }
    }
}
