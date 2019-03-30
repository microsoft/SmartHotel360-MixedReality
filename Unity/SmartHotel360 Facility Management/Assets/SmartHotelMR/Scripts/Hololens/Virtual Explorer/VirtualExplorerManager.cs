using HoloToolkit.UX.Progress;
using SmartHotelMR;
using System.Collections;
using System.Linq;
using UnityEngine;

public class VirtualExplorerManager : VirtualExplorerManagerBase
{
    [SerializeField]
    [Tooltip("Back button for navigating to previous Space.")]
    public GameObject BackButton;

    [SerializeField]
    [Tooltip("Gaze Cursor")]
    public GameObject Cursor;

    [SerializeField]
    [Tooltip("ProgressIndicator prefab")]
    public GameObject ProgressPrefab;

    public override void Update()
    {
        base.Update();

#if !UNITY_EDITOR
        if (Cursor != null)
            Cursor.SetActive(!SmartHotelManager.Instance.IsAdminMode);
#endif
    }

    protected override IEnumerator EnableBackButton(GameObject collection, bool enabled)
    {
        if (BackButton != null && collection != null)
        {
            yield return new WaitForEndOfFrame();

            BackButton.SetActive(enabled);

            yield return new WaitForEndOfFrame();

            var collectionBounds = GetCollectionBounds(collection);
            var buttonBounds = BackButton.GetComponent<Collider>().bounds;

            float x = collectionBounds.center.x;
            float y = collectionBounds.max.y + (buttonBounds.size.y * 3f);
            float z = collectionBounds.center.z;

            BackButton.transform.position = new Vector3(x, y, z);
        }
    }

    public override void ShowLoadingIndicator(string message)
    {
        if (ProgressIndicator.Instance == null)
            GameObject.Instantiate(ProgressPrefab);

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

    protected override void AddInteractionComponents(SmartHotelMR.Space space, GameObject instance)
    {
        if (_stateManager.SelectedSpace == null || _stateManager.SelectedSpace.type != DataManager.RoomType)
        {
            instance.AddComponent<SpaceInputReceiver>();
        }
        else
        {
            instance.AddComponent<SensorIndicatorInputReceiver>();
        }
    }
}
