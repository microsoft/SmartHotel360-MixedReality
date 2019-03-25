using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using SmartHotelMR;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpaceInputReceiver : MonoBehaviour, IInputClickHandler
{
    private DoubleClickPreventer _doubleClickPreventer = new DoubleClickPreventer(0.5f);

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (eventData.used)
            return;

        if (!_doubleClickPreventer.CanClick())
            return;

        Debug.Log(gameObject.name + " : InputClicked");

        eventData.Use();

        HandleSpacePressed();
    }

    private void HandleSpacePressed()
    {
        var spaceContext = gameObject.GetComponent<ISpaceContext>();

        if (spaceContext != null)
        {
            ExecuteEvents.ExecuteHierarchy<ISpaceMessageTarget>(gameObject, null, (x, y) => x.OnSpaceSelected(spaceContext.Context));
        }
    }
}
