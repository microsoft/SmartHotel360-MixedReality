using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public interface IAnchorMessageTarget : IEventSystemHandler
    {
        void OnSetVirtualExplorerAnchor(GameObject anchorObject);
        void OnSetPhysicalVisualizerAnchor(SpawnData spawnData);
    }
}
