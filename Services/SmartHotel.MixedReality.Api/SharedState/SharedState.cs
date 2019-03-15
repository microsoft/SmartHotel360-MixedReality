using System.Collections.Generic;
using NodaTime;

namespace SmartHotel.MixedReality.Api.SharedState
{
    public class SharedState
    {
        public string Id { get; set; }
        public string CurrentSelectedSpace { get; set; }
        public Dictionary<string, bool> ToggledSensorPanels { get; set; }
        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
    }
}