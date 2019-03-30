using HoloToolkit.Unity.InputModule;
using SmartHotelMR;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackButtonReceiver : MonoBehaviour, IInputClickHandler
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

        HandleBackButtonPress();
    }

    private void HandleBackButtonPress()
    {
        var dataManager = gameObject.GetComponentInParent<DataManager>();
        var stateManager = gameObject.GetComponentInParent<StateManager>();

        if (stateManager != null && dataManager != null)
        {
            SmartHotelMR.Space parentSpace = stateManager.SelectedSpace == null ? null : dataManager.GetSpaceById(stateManager.SelectedSpace.parentSpaceId);
            ExecuteEvents.ExecuteHierarchy<ISpaceMessageTarget>(gameObject, null, (x, y) => x.OnSpaceSelected(parentSpace));
        }
    }
}
