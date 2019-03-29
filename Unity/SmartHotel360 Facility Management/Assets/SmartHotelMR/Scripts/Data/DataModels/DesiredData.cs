using System;

namespace SmartHotelMR
{
    [Serializable]
    public class DesiredData
    {
        public string id;
        public string sensorId;
        public string roomId;
        public string desiredValue;
    }

    [Serializable]
    public class DesiredDataWrapper
    {
        public DesiredData[] values;
    }
}
