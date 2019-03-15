using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputManager : MonoBehaviour, IPointerClickHandler
{
    private bool holding = false;
    private PointerEventData lastPointerEventData;

    #region Events
    public delegate void PointerEventHandler(PointerEventData data);

    public event PointerEventHandler OnPointerClick = delegate { };

    public event PointerEventHandler OnPointerDown = delegate { };
    public event PointerEventHandler OnPointerHold = delegate { };
    public event PointerEventHandler OnPointerUp = delegate { };

    public event PointerEventHandler OnBeginDrag = delegate { };
    public event PointerEventHandler OnDrag = delegate { };
    public event PointerEventHandler OnEndDrag = delegate { };
    public event PointerEventHandler OnScroll = delegate { };
    #endregion

    public void Enable(bool enabled)
    {
        this.gameObject.SetActive(enabled);
    }

    #region Interface Implementations
    void IPointerClickHandler.OnPointerClick(PointerEventData e)
    {
        lastPointerEventData = e;
        OnPointerClick(e);
    }

    // And other interface implementations, you get the point
    #endregion

    void Update()
    {
        if (holding)
        {
            OnPointerHold(lastPointerEventData);
        }
    }
}