using System.Collections.Generic;

namespace SmartHotel.MixedReality.Api.Anchors
{
    public class AnchorSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Anchor> Anchors { get; set; }
    }
}