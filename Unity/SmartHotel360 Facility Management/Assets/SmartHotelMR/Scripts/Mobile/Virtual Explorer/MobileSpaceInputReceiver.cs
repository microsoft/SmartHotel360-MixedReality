using SmartHotelMR;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileSpaceInputReceiver : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("MobileSpaceInputReceiver::OnPointerClick - " + eventData.selectedObject.name);

        HandleSpacePressed();
    }

    public void HandleSpacePressed()
    {
        var spaceContext = gameObject.GetComponent<ISpaceContext>();

        if (spaceContext != null)
        {
            ExecuteEvents.ExecuteHierarchy<ISpaceMessageTarget>(gameObject, null, (x, y) => x.OnSpaceSelected(spaceContext.Context));
        }
    }
}