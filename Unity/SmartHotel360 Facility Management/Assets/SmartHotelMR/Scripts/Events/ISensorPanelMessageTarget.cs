using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public interface ISensorPanelMessageTarget : IEventSystemHandler
    {
        void OnSensorPanelToggled(string sensorId, bool state);
    }
}
