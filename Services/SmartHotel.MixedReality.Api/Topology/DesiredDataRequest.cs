using System.Runtime.Serialization;

namespace SmartHotel.MixedReality.Api.Topology
{
    [DataContract(Name = "desiredData")]
    public class DesiredDataRequest
    {
        [DataMember(Name = "roomId")]
        public string RoomId { get; set; }
        [DataMember(Name = "sensorId")]
        public string SensorId { get; set; }
        [DataMember(Name = "desiredValue")]
        public string DesiredValue { get; set; }
        [DataMember(Name = "methodName")]
        public string MethodName { get; set; }
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }
    }
}
