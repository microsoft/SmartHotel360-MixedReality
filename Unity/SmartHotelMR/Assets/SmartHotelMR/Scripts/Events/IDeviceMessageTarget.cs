using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public interface IDeviceMessageTarget : IEventSystemHandler
    {
        void OnDeviceSelected(Device device);
    }
}
