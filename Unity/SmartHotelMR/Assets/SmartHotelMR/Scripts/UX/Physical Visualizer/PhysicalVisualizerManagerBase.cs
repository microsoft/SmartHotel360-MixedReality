using Microsoft.Azure.SpatialAnchors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    [RequireComponent(typeof(AnchorManager))]
    [RequireComponent(typeof(DataManager))]
    public class PhysicalVisualizerManagerBase : MonoBehaviour
    {
        protected enum PVMode
        {
            Placement,
            Delete
        }

        protected bool _isInitialized;
        protected AnchorManager _anchorManager;
        protected DataManager _dataManager;
        protected Dictionary<string, GameObject> _knownAnchors = new Dictionary<string, GameObject>();

        protected PVMode Mode { get; set; }

        protected GameObject SelectedAnchor { get; set; }

        [SerializeField]
        [Tooltip("Content to show when in Admin Mode")]
        public GameObject AdminContent;

        [SerializeField]
        [Tooltip("Content to show when in User Mode and no anchor is set")]
        public GameObject NoAnchorContent;

        [SerializeField]
        [Tooltip("Prefab to use when placing/displaying anchors")]
        public GameObject SensorAnchorPrefab;

        [SerializeField]
        [Tooltip("Prefab to use for showing Device and Sensor data")]
        public GameObject DevicePanelPrefab;

        [SerializeField]
        [Tooltip("Object used for emailing Debug logs (only visible in debug builds)")]
        public GameObject EmailDebugLogObject;

#if UNITY_ANDROID || UNITY_IOS
        [SerializeField]
        [Tooltip("Object used to display status messages")]
        public Text StatusObject;
#else
        [SerializeField]
        [Tooltip("Text object used to show status and error messages")]
        public TextMesh StatusObject;
#endif

        public virtual void Start()
        {
            Mode = PVMode.Placement;

            _anchorManager = GetComponent<AnchorManager>();
            _dataManager = GetComponent<DataManager>();

            ShowLoadingIndicator("Loading Spaces...");

            //#if DEBUG
            //            if (EmailDebugLogObject != null)
            //                EmailDebugLogObject.SetActive(true);
            //#endif
        }

        public virtual void Update()
        {
#if UNITY_EDITOR
            if (SmartHotelManager.Instance == null)
                return;
#endif
            if (!_isInitialized)
            {
                StartCoroutine(_dataManager.Initialize());
                _isInitialized = true;
            }

            if (AdminContent != null)
                AdminContent.SetActive(SmartHotelManager.Instance.IsAdminMode);
        }

        public virtual IEnumerator HideLoadingIndicator()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ShowLoadingIndicator(string message)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnDataInitialized()
        {
            ShowLoadingIndicator("Loading Anchors...");
            StartCoroutine(_anchorManager.LoadPhysicalVisualizerAnchors(AnchorSetManager.Instance.SelectedAnchorSet.id));
        }

        public virtual void OnAnchorsLoaded()
        {
            if (_anchorManager.IsLocating)
            {
                ShowLoadingIndicator("Locating anchor(s), please walk around your environment");
            }
            else
            {
                StartCoroutine(HideLoadingIndicator());
            }

            if (NoAnchorContent != null)
                NoAnchorContent.SetActive(!_anchorManager.IsLocating && !SmartHotelManager.Instance.IsAdminMode);

            Debug.Log("PhysicalVisualizerManagerBase::OnAnchorsLoaded");
        }

        public virtual void OnAnchorsLocated()
        {
            StartCoroutine(HideLoadingIndicator());

            if (NoAnchorContent != null)
                NoAnchorContent.SetActive(!SmartHotelManager.Instance.IsAdminMode && !_anchorManager.HasAnchors());

            if (!SmartHotelManager.Instance.IsAdminMode)
                _anchorManager.StopAnchorService();
        }

        public virtual void OnAnchorLocated(AnchorLocatedResult result)
        {
            Debug.Log(string.Format("PhysicalVisualizerManagerBase::OnAnchorLocated - Position: {0}, Rotation: {1}",
                string.Format("({0}, {1}, {2})", result.SpawnData.Position.x, result.SpawnData.Position.y, result.SpawnData.Position.z),
                string.Format("({0}, {1}, {2})", result.SpawnData.Rotation.x, result.SpawnData.Rotation.y, result.SpawnData.Rotation.z)));

            if (result.Anchor == null)
                return;

            GameObject sensor;

            if (_knownAnchors.ContainsKey(result.Anchor.id))
            {
                sensor = _knownAnchors[result.Anchor.id];

                //Update sensor location
                sensor.transform.position = result.SpawnData.Position;
#if WINDOWS_UWP
                sensor.transform.localRotation = result.SpawnData.Rotation * Quaternion.AngleAxis(90f, Vector3.right);
#else
                sensor.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.right);
#endif
            }
            else
            {
                sensor = CreateSensorIndicator(result.SpawnData);
                _knownAnchors.Add(result.Anchor.id, sensor);

                _anchorManager.ApplyPhysicalVisualizerAnchor(sensor, result.CloudAnchor);

                SetupAnchor(sensor, result.Anchor.deviceId, result.CloudAnchor);
            }
        }

        public virtual void OnAnchorPlaced(AnchorPlacedResult result)
        {
            Debug.Log("PhysicalVisualizerManagerBase::OnAnchorPlaced");

            if (!string.IsNullOrEmpty(result.AssociatedId))
            {
                SetupAnchor(result.AnchorObject, result.AssociatedId, result.CloudAnchor);
            }
        }

        public virtual void OnAnchorDeleted()
        {
            SelectedAnchor = null;
            StartCoroutine(HideLoadingIndicator());
        }

        public virtual void OnSpawnAnchor(SpawnData spawnData)
        {
            if (Mode == PVMode.Delete)
                return;

            Debug.Log("PhysicalVisualizerManagerBase::OnSpawnAnchor");

            _anchorManager.IsPaused = true;

            var sensor = CreateSensorIndicator(spawnData);

            ShowChooser(sensor);
        }

        protected virtual GameObject CreateSensorIndicator(SpawnData spawnData)
        {
            var sensor = Instantiate(SensorAnchorPrefab, gameObject.transform);
            sensor.transform.position = spawnData.Position;
#if WINDOWS_UWP
            sensor.transform.localRotation = spawnData.Rotation * Quaternion.AngleAxis(90f, Vector3.right);
#else
            sensor.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.right);
#endif
            sensor.SetActive(true);

            return sensor;
        }

        protected virtual void ShowChooser(GameObject anchor)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnChooserClosed(ChooserResult result)
        {
            if (!result.Cancelled)
            {
                _anchorManager.AddPhysicalVisualizerAnchor(result.Anchor, result.SelectedDevice.id);
            }

            _anchorManager.IsPaused = false;
        }

        protected void SetupAnchor(GameObject anchor, string deviceId, CloudSpatialAnchor cloudAnchor)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                Debug.LogError("PhysicalVisualizerManagerBase::SetupAnchor - DeviceId is null");
                return;
            }

            var panel = Instantiate(DevicePanelPrefab, gameObject.transform);
            panel.SetActive(false);
            panel.transform.position = new Vector3(anchor.transform.position.x,
                anchor.transform.position.y + 0.3f, anchor.transform.position.z);

            var binding = panel.GetComponent<DeviceBinding>();
            binding.DeviceId = deviceId;

            var proximity = anchor.AddComponent<ProximityVisibility>();
            proximity.ProximityDistance = 1.1f;
            proximity.Object = panel;

            var anchorBinding = anchor.AddComponent<AnchorBinding>();
            anchorBinding.Anchor = cloudAnchor;
        }

        public virtual void SetAnchorStatusMessage(string message)
        {
            if (StatusObject != null)
                StatusObject.text = message;
        }

        public virtual void OnSetDeleteMode(bool activate)
        {
            if (!activate || Mode == PVMode.Delete || !SmartHotelManager.Instance.IsAdminMode)
                return;

            Mode = PVMode.Delete;
            _anchorManager.StopAnchorService(false);
            SetAnchorStatusMessage("Select an achor to delete");
        }

        public virtual void OnSetPlacementMode(bool activate)
        {
            if (!activate || Mode == PVMode.Placement || !SmartHotelManager.Instance.IsAdminMode)
                return;

            Mode = PVMode.Placement;
            SetAnchorStatusMessage(string.Empty);
            _anchorManager.StartAnchorService();
        }

        public virtual void OnExitAdminMode()
        {
            _anchorManager.StopAnchorService(true);
            SetAnchorStatusMessage(string.Empty);
            SmartHotelManager.Instance.IsAdminMode = false;

            if (NoAnchorContent != null)
                NoAnchorContent.SetActive(!SmartHotelManager.Instance.IsAdminMode && !_anchorManager.HasAnchors());
        }

        public void OnEmailClicked()
        {
#if DEBUG && !WINDOWS_UWP
            ShowLoadingIndicator("Getting log data...");

            StartCoroutine(DebugLogService.Instance.GetLogText((log) =>
            {
                Application.OpenURL(string.Format("mailto:?subject=DebugLog&body={0}", UnityEngine.Networking.UnityWebRequest.EscapeURL(log)));

                StartCoroutine(HideLoadingIndicator());
            }));
#endif
        }
    }
}