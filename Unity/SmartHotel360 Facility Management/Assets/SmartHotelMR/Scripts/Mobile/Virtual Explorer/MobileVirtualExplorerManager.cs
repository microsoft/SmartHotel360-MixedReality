using SmartHotelMR;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileVirtualExplorerManager : VirtualExplorerManagerBase
{
    [Tooltip("Object to show in Admin mode for placing the anchor")]
    public GameObject AnchorObject;

    [Tooltip("Object to show in Admin mode when you place the anchor")]
    public GameObject AnchorPrefab;

    [SerializeField]
    [Tooltip("Back button for navigating to previous Space.")]
    public GameObject BackButton;

    [SerializeField]
    [Tooltip("Content to show when loading")]
    public GameObject LoadingIndicator;

    [SerializeField]
    [Tooltip("Object used to show loading message(s)")]
    public Text LoadingMessageLabel;

    [SerializeField]
    public Text LoadingMessageLabelShadow;

    [SerializeField]
    public GameObject MenuButton;

    [SerializeField]
    [Tooltip("Input manager required for processing mobile touches")]
    public MobileInputManager InputManager;

    public override void Start()
    {
        base.Start();

        if (InputManager != null)
            InputManager.OnPointerClick += OnPointerClick;

        if (AnchorObject != null)
            AnchorObject.SetActive(SmartHotelManager.Instance.IsAdminMode);
    }

    private void OnDestroy()
    {
        if (InputManager != null)
            InputManager.OnPointerClick -= OnPointerClick;
    }

    public override void Update()
    {
        base.Update();

        if (MenuButton != null)
            MenuButton.SetActive(!SmartHotelManager.Instance.IsAdminMode);
    }

    public override void OnAnchorsLoaded()
    {
        base.OnAnchorsLoaded();

        if (!SmartHotelManager.Instance.IsAdminMode && _anchorManager.IsLocating)
            ShowLoadingIndicator("Locating anchor, please look around.");
    }

    protected void OnPointerClick(PointerEventData eventData)
    {
        if (SmartHotelManager.Instance.IsAdminMode)
        {
            if (_anchorManager.IsSaving || _anchorManager.IsLocating)
                return;

            if (AnchorPrefab != null)
            {
                var obj = GameObject.Instantiate<GameObject>(AnchorPrefab);
                obj.transform.position = AnchorObject.transform.position;
                obj.SetActive(true);

                AnchorObject.SetActive(false);

                ExecuteEvents.ExecuteHierarchy<IAnchorMessageTarget>(gameObject, null, (x, y) => x.OnSetVirtualExplorerAnchor(obj));
            }
        }
        else
        {
            var touchPosition = eventData.pressPosition;
            var ray = Camera.main.ScreenPointToRay(touchPosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject != null)
                {
                    GameObject touchedObject = hit.transform.gameObject;
                    Debug.Log("MobileVirtualExplorerManager::OnPointerClick - Touched " + touchedObject.transform.name);

                    var spaceInputReceiver = touchedObject.GetComponent<MobileSpaceInputReceiver>();

                    if (spaceInputReceiver != null)
                    {
                        spaceInputReceiver.HandleSpacePressed();
                    }
                    else
                    {
                        var sensorInputReceiver = touchedObject.GetComponentInParent<MobileSensorIndicatorInputReceiver>();

                        if (sensorInputReceiver != null)
                        {
                            sensorInputReceiver.HandleIndicatorPressed(touchedObject);
                        }
                    }
                }
                else
                {
                    Debug.Log("MobileVirtualExplorerManager::OnPointerClick - SelectedObject is null");
                }
            }
        }
    }

    public void OnBackButtonClicked()
    {
        SmartHotelMR.Space parentSpace = _stateManager.SelectedSpace == null ? null : _dataManager.GetSpaceById(_stateManager.SelectedSpace.parentSpaceId);

        Debug.Log("MobileVirtualExplorerManager::OnBackButtonClicked - " + (parentSpace == null ? "null" : parentSpace.friendlyName));

        ExecuteEvents.ExecuteHierarchy<ISpaceMessageTarget>(gameObject, null, (x, y) => x.OnSpaceSelected(parentSpace));
    }

    public void OnSaveAnchorFailed(GameObject anchor)
    {
        Destroy(anchor);

        AnchorObject.SetActive(true);
    }

    protected override IEnumerator PlaceAnchor(GameObject anchor)
    {
        yield return base.PlaceAnchor(anchor);

        gameObject.transform.position = anchor.transform.position;
        gameObject.transform.rotation = anchor.transform.rotation;

        Destroy(anchor);
    }

    protected override IEnumerator EnableBackButton(GameObject collection, bool enabled)
    {
        yield return new WaitForEndOfFrame();

        if (BackButton != null && collection != null)
        {
            BackButton.SetActive(enabled);
        }
    }

    public override void ShowLoadingIndicator(string message)
    {
        if (LoadingMessageLabel != null)
        {
            LoadingMessageLabel.text = message;
        }

        if (LoadingMessageLabelShadow != null)
        {
            LoadingMessageLabelShadow.text = message;
        }

        if (LoadingIndicator != null)
        {
            LoadingIndicator.SetActive(true);
        }
    }

    public override IEnumerator HideLoadingIndicator()
    {
        if (LoadingIndicator != null)
        {
            LoadingIndicator.SetActive(false);
        }

        yield return new WaitForEndOfFrame();
    }

    private Vector3 GetCollectionExtents(GameObject collection)
    {
        var colliders = collection.GetComponentsInChildren<Collider>();

        if (colliders != null && colliders.Any())
        {
            Bounds collectiveBounds = colliders[0].bounds;

            foreach (var collider in colliders.Skip(1))
            {
                collectiveBounds = collectiveBounds.ExpandToContain(collider.bounds);
            }

            return collectiveBounds.extents;
        }

        return Vector3.zero;
    }

    protected override void AddInteractionComponents(SmartHotelMR.Space space, GameObject instance)
    {
        if (_stateManager.SelectedSpace == null || _stateManager.SelectedSpace.type != DataManager.RoomType)
        {
            instance.AddComponent<MobileSpaceInputReceiver>();
        }
        else
        {
            instance.AddComponent<MobileSensorIndicatorInputReceiver>();
        }
    }
}
