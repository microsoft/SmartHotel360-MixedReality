using System;
using System.Collections.Generic;

namespace SmartHotelMR
{
    [Serializable]
    public class AnchorSet
    {
        public string id;
        public string name;
        public List<Anchor> anchors;
    }


    [Serializable]
    public class AnchorSetWrapper
    {
        public AnchorSet[] values;
    }
}