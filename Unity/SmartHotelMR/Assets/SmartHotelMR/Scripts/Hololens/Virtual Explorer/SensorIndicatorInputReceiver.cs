using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class SensorIndicatorInputReceiver : MonoBehaviour, IInputClickHandler
{
    private DoubleClickPreventer _doubleClickPreventer = new DoubleClickPreventer(0.5f);

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (eventData.used)
            return;

        if (!_doubleClickPreventer.CanClick())
            return;

        eventData.Use();

        HandleIndicatorPressed(eventData.selectedObject);
    }

    private void HandleIndicatorPressed(GameObject indicator)
    {
        gameObject.SendMessage("OnIndicatorSelected", indicator);
    }
}
