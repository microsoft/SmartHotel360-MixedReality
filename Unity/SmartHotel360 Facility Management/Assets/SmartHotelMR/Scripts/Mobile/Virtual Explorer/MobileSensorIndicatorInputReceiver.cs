using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSensorIndicatorInputReceiver : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("MobileSensorIndicatorInputReceiver::OnPointerClick - " + eventData.selectedObject.name);

        HandleIndicatorPressed(eventData.selectedObject);
    }

    public void HandleIndicatorPressed(GameObject indicator)
    {
        gameObject.SendMessage("OnIndicatorSelected", indicator);
    }
}
