using HoloToolkit.Unity.InputModule;
using HoloToolkit.UX.Dialog;
using HoloToolkit.UX.Progress;
using SmartHotelMR;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DigitalTwinsChooser))]
public class HololensPhysicalVisualizerManager : PhysicalVisualizerManagerBase, IInputClickHandler
{
    private DigitalTwinsChooser _chooser;

    [SerializeField]
    [Tooltip("Content to show when loading")]
    public GameObject LoadingIndicator;

    [SerializeField]
    [Tooltip("Gaze Cursor")]
    public GameObject Cursor;

    [SerializeField]
    [Tooltip("Prefab for delete confirmation")]
    public Dialog DialogPrefab;

    [SerializeField]
    [Tooltip("Anchor Cursor")]
    public GameObject AnchorCursor;

    public override void Start()
    {
        if (ProgressIndicator.Instance == null)
            GameObject.Instantiate(LoadingIndicator);

        base.Start();

        _chooser = GetComponent<DigitalTwinsChooser>();
    }

    void OnEnable()
    {
        InputManager.Instance.AddGlobalListener(gameObject);

        AnchorCursor.SetActive(false);

        if (SmartHotelManager.Instance.IsAdminMode)
            Cursor.SetActive(false);
    }

    void OnDisable()
    {
        InputManager.Instance.RemoveGlobalListener(gameObject);
    }

    public override void Update()
    {
        bool hideAnchorCursor = false;

        if (!_isInitialized)
            hideAnchorCursor = true;

        base.Update();

        if (hideAnchorCursor)
        {
            AnchorCursor.SetActive(false);
            Cursor.SetActive(false);
        }

        if (!SmartHotelManager.Instance.IsAdminMode)
        {
            AnchorCursor.SetActive(false);
            Cursor.SetActive(true);
        }
    }

    public override void ShowLoadingIndicator(string message)
    {
        StartCoroutine(ShowLoadingIndicatorAsync(message));
    }

    private IEnumerator ShowLoadingIndicatorAsync(string message)
    {
        yield return new WaitForEndOfFrame();

        if (ProgressIndicator.Instance == null)
            GameObject.Instantiate(LoadingIndicator);

        ProgressIndicator.Instance.Open(IndicatorStyleEnum.AnimatedOrbs, ProgressStyleEnum.None, ProgressMessageStyleEnum.Visible, message);
    }

    public override IEnumerator HideLoadingIndicator()
    {
        if (ProgressIndicator.Instance == null)
            yield break;

        ProgressIndicator.Instance.Close();
        while (ProgressIndicator.Instance.IsLoading)
        {
            yield return null;
        }
    }

    public override void OnAnchorsLoaded()
    {
        base.OnAnchorsLoaded();

        if (SmartHotelManager.Instance.IsAdminMode)
        {
            Cursor.SetActive(false);

            if (!_anchorManager.IsLocating)
                AnchorCursor.SetActive(true);
        }
    }

    public override void OnAnchorsLocated()
    {
        base.OnAnchorsLocated();

        if (SmartHotelManager.Instance.IsAdminMode)
        {
            Cursor.SetActive(false);
            AnchorCursor.SetActive(true);
        }
    }

    protected override void ShowChooser(GameObject anchor)
    {
        StatusObject.gameObject.SetActive(false);

        Debug.LogWarning("HololensPhysicalVisualizerManager::ShowChooser");

        Cursor.SetActive(true);
        AnchorCursor.SetActive(false);

        var chooser = GetComponent<DigitalTwinsChooser>();
        chooser.Show(anchor);
    }

    protected override void OnChooserClosed(ChooserResult result)
    {
        StatusObject.gameObject.SetActive(true);

        base.OnChooserClosed(result);

        AnchorCursor.SetActive(true);
        Cursor.SetActive(false);
    }

    public override void OnSetPlacementMode(bool activate)
    {
        base.OnSetPlacementMode(activate);

        AnchorCursor.SetActive(true);
        Cursor.SetActive(false);
    }

    public override void OnSetDeleteMode(bool activate)
    {
        base.OnSetDeleteMode(activate);

        AnchorCursor.SetActive(false);
        Cursor.SetActive(true);
    }

    public override void OnExitAdminMode()
    {
        if (!SmartHotelManager.Instance.IsAdminMode)
            return;

        base.OnExitAdminMode();

        AnchorCursor.SetActive(false);
        Cursor.SetActive(true);

        SetAnchorStatusMessage(string.Empty);
    }

    protected IEnumerator UpdateSelectionStates(GameObject selectedObject)
    {
        var objects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];

        yield return new WaitForEndOfFrame();

        foreach (var obj in objects)
        {
            if (obj == selectedObject)
                continue;

            var selectable = obj.GetComponent<SelectableSensor>();

            if (selectable == null)
                continue;

            selectable.IsSelected = false;

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (Mode != PVMode.Delete || SelectedAnchor != null)
            return;

        if (eventData.selectedObject != null)
        {
            GameObject touchedObject = eventData.selectedObject;
            Debug.Log("HololensPhysicalVisualizerManager::OnPointerClick - Touched " + touchedObject.transform.name);

            var selectable = touchedObject.GetComponent<SelectableSensor>();

            if (selectable == null)
            {
                Debug.Log("HololensPhysicalVisualizerManager::OnPointerClick - Selectable sensor not found" + touchedObject.transform.name);
                return;
            }

            selectable.IsSelected = true;

            StartCoroutine(UpdateSelectionStates(touchedObject));

            SelectedAnchor = touchedObject;

            string message = "Are you sure you want to delete this anchor?";

            Dialog dialog = Dialog.Open(DialogPrefab.gameObject, DialogButtonType.Yes | DialogButtonType.No, "Delete Anchor", message);
            dialog.OnClosed += OnDeleteDialogClosed;
        }
    }

    private void OnDeleteDialogClosed(DialogResult result)
    {
        if (result.Result == DialogButtonType.Yes)
        {
            ShowLoadingIndicator("Deleting anchor...");

            var panel = SelectedAnchor.GetComponent<ProximityVisibility>().Object;
            var cloudAnchor = SelectedAnchor.GetComponent<AnchorBinding>().Anchor;

            _anchorManager.DeleteAnchor(SelectedAnchor, panel, cloudAnchor);
        }

        SelectedAnchor = null;
        StartCoroutine(UpdateSelectionStates(null));
    }
}
