using System;

namespace SmartHotelMR
{
    [Serializable]
    public class Sensor
    {
        public string id;
        public string deviceId;
        public int dataTypeId;
        public string dataType;
        public int dataSubtypeId;
        public string dataSubtype;
        public int typeId;
        public string type;
        public string spaceId;
    }
}