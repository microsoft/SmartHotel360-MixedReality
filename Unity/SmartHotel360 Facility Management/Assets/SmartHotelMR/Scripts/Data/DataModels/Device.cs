using System;
using System.Collections.Generic;

namespace SmartHotelMR
{
    [Serializable]
    public class Device
    {
        public string id;
        public string name;
        public string hardwareId;
        public int typeId;
        public string subtype;
        public int subtypeId;
        public string spaceId;
        public string status;
        public List<Sensor> sensors;
    }
}