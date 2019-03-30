using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public interface ISpaceMessageTarget : IEventSystemHandler
    {
        void OnSpaceSelected(Space space);
    }
}
