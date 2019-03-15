using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonRepeater : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("Time before repeating button press")]
    public float RepeatDelay = 1f;

    [Tooltip("How often to repeat button press")]
    public float RepeatInterval = 0.1f;

    private Button _button;
    private PointerEventData _pointerData = null;
    private float _lastTrigger = 0;

    private void Awake()
    {
        _button = transform.GetComponent<Button>();
    }

    private void Update()
    {
        if (_pointerData != null)
        {
            if (Time.realtimeSinceStartup - _lastTrigger >= RepeatDelay)
            {
                _lastTrigger = Time.realtimeSinceStartup - (RepeatDelay - RepeatInterval);
                _button.OnSubmit(_pointerData);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _lastTrigger = Time.realtimeSinceStartup;
        _pointerData = eventData;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pointerData = null;
        _lastTrigger = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerData = null;
        _lastTrigger = 0f;
    }
}