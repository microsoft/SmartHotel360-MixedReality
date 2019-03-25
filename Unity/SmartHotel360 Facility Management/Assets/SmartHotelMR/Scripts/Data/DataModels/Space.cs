using System;
using System.Collections.Generic;

namespace SmartHotelMR
{
    [Serializable]
    public class Space
    {
        public Space()
        {
            childSpaces = new List<Space>();
        }

        public string id;
        public string parentSpaceId;
        public string name;
        public string friendlyName;
        public string type;
        public int typeId;
        public string subtype;
        public int subtypeId;
        public List<Property> properties;
        public List<Space> childSpaces;
        public List<Value> values;
        public List<Device> devices;
    }
}