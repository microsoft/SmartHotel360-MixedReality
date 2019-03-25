using SmartHotelMR;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(DigitalTwinsChooser))]
public class MobilePhysicalVisualizerManager : PhysicalVisualizerManagerBase
{
    private DigitalTwinsChooser _chooser;

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

    [SerializeField]
    [Tooltip("Prefab for delete confirmation")]
    public GameObject ConfirmationDialogPrefab;

    [SerializeField]
    [Tooltip("Object used for displaying dialogs")]
    public Transform DialogParent;

    public override void Start()
    {
        base.Start();

        _chooser = GetComponent<DigitalTwinsChooser>();

        if (InputManager != null)
            InputManager.OnPointerClick += OnPointerClick;
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

    protected override void ShowChooser(GameObject anchor)
    {
        Debug.LogWarning("MobilePhysicalVisualizerManager::ShowChooser");

        var chooser = GetComponent<DigitalTwinsChooser>();
        chooser.Show(anchor);
    }

    protected void OnPointerClick(PointerEventData eventData)
    {
        if (Mode != PVMode.Delete)
            return;

        var touchPosition = eventData.pressPosition;
        var ray = Camera.main.ScreenPointToRay(touchPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject != null)
            {
                GameObject touchedObject = hit.transform.gameObject;
                Debug.Log("MobilePhysicalVisualizerManager::OnPointerClick - Touched " + touchedObject.transform.name);

                var selectable = touchedObject.GetComponent<SelectableSensor>();

                if (selectable == null)
                {
                    Debug.Log("MobilePhysicalVisualizerManager::OnPointerClick - Selectable sensor not found" + touchedObject.transform.name);
                    return;
                }

                selectable.IsSelected = true;

                StartCoroutine(UpdateSelectionStates(touchedObject));

                SelectedAnchor = touchedObject;

                string message = "Are you sure you want to delete this anchor?";

                ConfirmationDialog.ShowDialog(ConfirmationDialogPrefab, DialogParent, message, (result) =>
                {
                    Debug.Log(string.Format("Delete dialog closed, result: {0}", result));

                    if (result && SelectedAnchor != null)
                    {
                        ShowLoadingIndicator("Deleting anchor...");

                        var panel = SelectedAnchor.GetComponent<ProximityVisibility>().Object;
                        var cloudAnchor = SelectedAnchor.GetComponent<AnchorBinding>().Anchor;

                        _anchorManager.DeleteAnchor(SelectedAnchor, panel, cloudAnchor);
                    }
                });
            }
        }
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

    public override void OnAnchorLocated(AnchorLocatedResult result)
    {
        if (_knownAnchors.Count == 0)
        {
            Debug.Log(string.Format("MobilePhysicalVisualizerManager::OnAnchorLocated - Position: {0}, Rotation: {1}",
                string.Format("({0}, {1}, {2})", result.SpawnData.Position.x, result.SpawnData.Position.y, result.SpawnData.Position.z),
                string.Format("({0}, {1}, {2})", result.SpawnData.Rotation.x, result.SpawnData.Rotation.y, result.SpawnData.Rotation.z)));

            //TODO: Google ARCore may require adjustment based on initial anchor in order to accomodate Unity's moving world origin
        }

        base.OnAnchorLocated(result);
    }

    private Pose _WorldToAnchorPose(Pose pose, Vector3 anchorPosition, Quaternion anchorRotation)
    {
        Matrix4x4 anchorTWorld = Matrix4x4.TRS(anchorPosition, anchorRotation, Vector3.one).inverse;

        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

        return new Pose(position, rotation);
    }
}
