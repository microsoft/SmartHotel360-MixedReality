using HoloToolkit.Unity.InputModule;
using SmartHotelMR;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnchorTapToPlace : MonoBehaviour, IInputClickHandler
{
    private DoubleClickPreventer _doubleClickPreventer = new DoubleClickPreventer(0.5f);

    void OnEnable()
    {
        InputManager.Instance.AddGlobalListener(gameObject);
    }

    void OnDisable()
    {
        InputManager.Instance.RemoveGlobalListener(gameObject);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (eventData.used)
            return;

        if (!_doubleClickPreventer.CanClick())
            return;

        eventData.Use();

        SetAnchor();
    }

    private void SetAnchor()
    {
        ExecuteEvents.ExecuteHierarchy<IAnchorMessageTarget>(gameObject, null, (x, y) => x.OnSetVirtualExplorerAnchor(gameObject));
    }
}
