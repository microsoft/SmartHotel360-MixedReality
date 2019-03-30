using System;
using System.Collections.Generic;

namespace SmartHotelMR
{
    [Serializable]
    public class State
    {
        public string id;
        public string currentSelectedSpace;
        public Dictionary<string, bool> toggledSensorPanels;
        //public string createdAt;
        //public string updatedAt;

        public State()
        {
            toggledSensorPanels = new Dictionary<string, bool>();
        }
    }
}
