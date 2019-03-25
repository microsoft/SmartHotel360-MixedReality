using System;

namespace SmartHotelMR
{
    [Serializable]
    public class SensorData
    {
        public string id;
        public string sensorId;
        public string roomId;
        public string sensorReading;
        public string sensorDataType;
        public string eventTimestamp;
        public string ioTHubDeviceId;
    }
}