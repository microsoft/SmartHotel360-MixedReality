using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using UnityEngine.EventSystems;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity.Samples;
#if UNITY_IOS
using UnityEngine.XR.iOS;
#elif WINDOWS_UWP
using UnityEngine.XR.WSA;
#elif UNITY_ANDROID
using GoogleARCore;
#endif

namespace SmartHotelMR
{
    public class AnchorManager : MonoBehaviour, IAnchorMessageTarget
    {
        private readonly string AnchorSetsRoute = "anchorsets";
        private readonly string VirtualExplorerRoute = "virtual";
        private readonly string PhysicalVisualizerRoute = "physical";

        private Queue<Action> _dispatchQueue = new Queue<Action>();
        private bool _enoughDataToCreate = false;
        private List<Anchor> _anchors = new List<Anchor>();
        private List<CloudSpatialAnchor> _cloudAnchors = new List<CloudSpatialAnchor>();

#if UNITY_IOS || UNITY_ANDROID
        [SerializeField]
        [Tooltip("Input manager required for processing mobile touches")]
        public MobileInputManager InputManager;
#endif

        public bool IsSaving { get; private set; }

        public bool IsLocating { get; private set; }

        private bool _isPaused = false;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
            }
        }

        void OnEnable()
        {
            SetStatusMessage(string.Empty);

            var cloudManager = AzureSpatialAnchorManager.Instance;

            cloudManager.OnSessionUpdated += CloudManager_SessionUpdated;
            cloudManager.OnAnchorLocated += CloudManager_OnAnchorLocated;
            cloudManager.OnLocateAnchorsCompleted += CloudManager_OnLocateAnchorsCompleted;

#if UNITY_IOS || UNITY_ANDROID
            if (InputManager != null)
                InputManager.OnPointerClick += OnPointerClick;
#endif
        }

        void OnDisable()
        {
            Cleanup();
        }

        void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            Debug.Log("AnchorManager::Cleanup");

            var cloudManager = AzureSpatialAnchorManager.Instance;

            if (cloudManager != null)
            {
                cloudManager.EnableProcessing = false;
                cloudManager.OnSessionUpdated -= CloudManager_SessionUpdated;
                cloudManager.OnAnchorLocated -= CloudManager_OnAnchorLocated;
                cloudManager.OnLocateAnchorsCompleted -= CloudManager_OnLocateAnchorsCompleted;
            }

#if UNITY_IOS || UNITY_ANDROID
            if (InputManager != null)
                InputManager.OnPointerClick -= OnPointerClick;
#endif
        }

        void Update()
        {
            lock (_dispatchQueue)
            {
                if (_dispatchQueue.Count > 0)
                {
                    _dispatchQueue.Dequeue()();
                }
            }
        }

        #region Global

        public void StartAnchorService()
        {
            try
            {

                    ConfigureSession();
                    AzureSpatialAnchorManager.Instance.EnableProcessing = true;

                    if (IsLocating)
                        AzureSpatialAnchorManager.Instance.CreateWatcher();
               
            }
            catch(Exception e)
            {
                Debug.Log("StartAnchorService :: Exception " + e.Message);
            }

        }

        public void StopAnchorService(bool clearStatus = true)
        {
            AzureSpatialAnchorManager.Instance.EnableProcessing = false;

            _enoughDataToCreate = false;
            IsSaving = false;
            IsLocating = false;

            if (clearStatus)
                SetStatusMessage(string.Empty);
        }

        private void ConfigureSession()
        {
            try
            {
                List<string> anchorsToFind = new List<string>(_anchors.Select(a => a.id));
                AzureSpatialAnchorManager.Instance.SetAnchorIdsToLocate(anchorsToFind);

            }
            catch (Exception e)
            {
                Debug.Log("ConfigureSession :: Exception: " + e.Message);
            }
        }

        private void CloudManager_SessionUpdated(object sender, SessionUpdatedEventArgs args)
        {
            _enoughDataToCreate = (args.Status.ReadyForCreateProgress >= 1);

            if (AzureSpatialAnchorManager.Instance.EnableProcessing && !IsSaving && !IsLocating)
                SetStatusMessage(_enoughDataToCreate ? "Tap to place anchor(s)" : "Move your device to capture more environment data");
        }

        private void CloudManager_OnLocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            IsLocating = false;
            Debug.Log("Locate pass complete");

            lock (_dispatchQueue)
            {
                _dispatchQueue.Enqueue(new Action(() =>
                {
                    BroadcastMessage("OnAnchorsLocated");
                }));
            }
        }

        private void CloudManager_OnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            Debug.LogFormat("anchor recognized as a possible anchor {0} {1}", args.Identifier, args.Status);
            if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
            {
                if (!_cloudAnchors.Contains(args.Anchor))
                    _cloudAnchors.Add(args.Anchor);

                lock (_dispatchQueue)
                {
                    _dispatchQueue.Enqueue(new Action(() =>
                    {
                        var result = new AnchorLocatedResult();
                        result.CloudAnchor = args.Anchor;
                        result.Anchor = _anchors.FirstOrDefault(a => a.id == args.Identifier);

                        Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                        anchorPose = args.Anchor.GetAnchorPose();
#endif
                        result.SpawnData = new SpawnData() { Position = anchorPose.position, Rotation = anchorPose.rotation };

                        BroadcastMessage("OnAnchorLocated", result);
                    }));
                }
            }
        }

        private void SetStatusMessage(string message)
        {
            lock (_dispatchQueue)
            {
                _dispatchQueue.Enqueue(new Action(() =>
                {
                    BroadcastMessage("SetAnchorStatusMessage", message);
                }));
            }
        }

        private IEnumerator CreateLocalAnchor(GameObject anchorObject, Action<CloudSpatialAnchor> callback)
        {
            lock (_dispatchQueue)
            {
                _dispatchQueue.Enqueue(new Action(() =>
                {
                    anchorObject.AddARAnchor();
                    StartCoroutine(CreateLocalAnchorInternal(anchorObject, callback));
                }));
            }
            yield return null;
           
        }

        private IEnumerator CreateLocalAnchorInternal(GameObject anchorObject, Action<CloudSpatialAnchor> callback)
        {
            yield return new WaitForSeconds(.5f);

            yield return new WaitWhile(() => anchorObject.GetNativeAnchorPointer() == null);
            yield return new WaitWhile(() => anchorObject.GetNativeAnchorPointer() == IntPtr.Zero);

            CloudSpatialAnchor localCloudAnchor = new CloudSpatialAnchor();
            localCloudAnchor.LocalAnchor = anchorObject.GetNativeAnchorPointer();

            if (localCloudAnchor.LocalAnchor == IntPtr.Zero)
            {
                Debug.LogError("Didn't get the local XR anchor pointer...");
            }

            callback(localCloudAnchor);
        }

        public async Task DeleteAnchor(GameObject anchorObject, GameObject relatedObject, CloudSpatialAnchor cloudAnchor)
        {
            try
            {
                var url = string.Format("{0}/{1}/{2}/{3}",
                    Globals.ServiceBaseUrl,
                    AnchorSetsRoute,
                    AnchorSetManager.Instance.SelectedAnchorSet.id,
                    cloudAnchor.Identifier);

#if !UNITY_EDITOR
                using (HttpClient client = new HttpClient())
                {
                    client.AddApiKeyHeader(Globals.ApiKey);
                    var result = await client.DeleteAsync(url);

                    if (result.IsSuccessStatusCode)
                    {
                        Debug.Log("AnchorManager::DeleteAnchor - Deleting cloud anchor...");

                        await AzureSpatialAnchorManager.Instance.DeleteAnchorAsync(cloudAnchor);

                        Debug.Log("AnchorManager::DeleteAnchor - Cloud anchor deleted");

                        lock (_dispatchQueue)
                        {
                            _dispatchQueue.Enqueue(new Action(() =>
                            {
                                Destroy(anchorObject);

                                if (relatedObject != null)
                                {
                                    Destroy(relatedObject);
                                }

                                BroadcastMessage("OnAnchorDeleted");
                            }));
                        }
                    }
                    else
                    {
                        Debug.LogError("AnchorManager::DeleteAnchor Failed - " + result.StatusCode + result.ReasonPhrase);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError("AnchorManager::DeleteAnchor Failed - " + ex.ToString());
            }
        }

        private async Task<bool> SaveAnchor(string url, string body = null)
        {
            try
            {
#if !UNITY_EDITOR
                using (HttpClient client = new HttpClient())
                {
                    client.AddApiKeyHeader(Globals.ApiKey);
                    var result = await client.PutAsync(url, string.IsNullOrWhiteSpace(body) ? null : new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json"));

                    return result.IsSuccessStatusCode;
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }

            return false;
        }

        public bool HasAnchors()
        {
            return _cloudAnchors != null && _cloudAnchors.Count > 0;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (AzureSpatialAnchorManager.Instance.EnableProcessing && !IsLocating && !IsPaused)
            {
                PositionLocalObject(eventData.pressPosition);
            }
        }

        void PositionLocalObject(Vector2 touchPosition)
        {
            if (!_enoughDataToCreate || IsPaused)
                return;

#if UNITY_IOS
            var screenPosition = Camera.main.ScreenToViewportPoint(touchPosition);
            ARPoint point = new ARPoint
            {
                x = screenPosition.x,
                y = screenPosition.y
            };

            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point,
                ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane | ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent);
            if (hitResults.Count > 0)
            {
                Vector3 pos = UnityARMatrixOps.GetPosition(hitResults[0].worldTransform);

                BroadcastMessage("OnSpawnAnchor", new SpawnData() { Position = pos, Rotation = Quaternion.AngleAxis(0, Vector3.up) });
            }
#elif UNITY_ANDROID
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touchPosition.x, touchPosition.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(Camera.main.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    Vector3 worldPos = hit.Pose.position;
                    //Quaternion worldRot = hit.Pose.rotation;
                    Quaternion worldRot = Quaternion.AngleAxis(0, Vector3.up);

                    BroadcastMessage("OnSpawnAnchor", new SpawnData() { Position = worldPos, Rotation = worldRot });
                }
            }
#endif
        }

        #endregion

        #region Virtual Explorer
        public IEnumerator LoadVirtualExplorerAnchor(string anchorSetId, bool ignoreExisting = false)
        {
            var url = string.Format("{0}/{1}/{2}/{3}",
                Globals.ServiceBaseUrl,
                AnchorSetsRoute,
                VirtualExplorerRoute,
                anchorSetId);

            using (var request = UnityWebRequest.Get(url))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    try
                    {
                        var anchorSet = JsonUtility.FromJson<AnchorSet>(request.downloadHandler.text);

                        if (!ignoreExisting && anchorSet != null && anchorSet.anchors != null && anchorSet.anchors.Any())
                        {
                            _anchors = anchorSet.anchors.ToList();
                            IsLocating = true;
                            Debug.Log("Locating anchor(s), please walk around your environment");
                            SetStatusMessage("Locating anchor(s), please walk around your environment");
                        }

                        StartAnchorService();

                        lock (_dispatchQueue)
                        {
                            _dispatchQueue.Enqueue(new Action(() =>
                            {
                                BroadcastMessage("OnAnchorsLoaded");
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("AnchorManager::LoadVirtualExplorerAnchor - Failed to load anchors: " + e.ToString());
                    }
                }
            }
        }

        private async Task<bool> SaveVirtualExplorerAnchor(string anchorSetId, CloudSpatialAnchor anchor)
        {
            var url = string.Format("{0}/{1}/{2}/{3}/{4}",
                Globals.ServiceBaseUrl,
                AnchorSetsRoute,
                VirtualExplorerRoute,
                anchorSetId,
                anchor.Identifier);

            return await SaveAnchor(url);
        }

        public void ApplyVirtualExplorerAnchor(GameObject anchorObject)
        {
            if (_cloudAnchors == null || !_cloudAnchors.Any())
                return;

            anchorObject.AddARAnchor();

#if WINDOWS_UWP
            anchorObject.GetComponent<WorldAnchor>().SetNativeSpatialAnchorPtr(_cloudAnchors.First().LocalAnchor);
#endif
        }

        public void OnSetVirtualExplorerAnchor(GameObject anchorObject)
        {
            if (!_enoughDataToCreate || IsPaused)
            {
                lock (_dispatchQueue)
                {
                    _dispatchQueue.Enqueue(new Action(() =>
                    {
                        BroadcastMessage("OnSaveAnchorFailed", anchorObject);
                    }));
                }

                return;
            }

            IsSaving = true;

            var rotationVector = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            var toRotation = Quaternion.LookRotation(rotationVector, Vector3.up);
            anchorObject.transform.rotation = toRotation;

            var anchorPosition = anchorObject.transform.position;

            StartCoroutine(CreateLocalAnchor(anchorObject,(cloudAnchor) =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        SetStatusMessage("Saving anchor...");

                        cloudAnchor = await AzureSpatialAnchorManager.Instance.StoreAnchorInCloud(cloudAnchor);

                        if (cloudAnchor != null)
                        {
                            Debug.Log("AnchorManager::OnSetVirtualExplorerAnchor - Cloud Anchor created");

                            bool success = await SaveVirtualExplorerAnchor(AnchorSetManager.Instance.SelectedAnchorSet.id, cloudAnchor);

                            if (success)
                            {
                                Debug.Log("AnchorManager::OnSetVirtualExplorerAnchor - Virtual Explorer Anchor created");

                                lock (_cloudAnchors)
                                {
                                    _cloudAnchors.Add(cloudAnchor);
                                }

                                SetStatusMessage(string.Empty);

                                lock (_dispatchQueue)
                                {
                                    _dispatchQueue.Enqueue(new Action(() =>
                                    {
                                        BroadcastMessage("OnAnchorPlaced",
                                            new AnchorPlacedResult()
                                            {
                                                AnchorObject = anchorObject,
                                                CloudAnchor = cloudAnchor
                                            });
                                    }));
                                }
                            }
                            else
                            {
                                await AzureSpatialAnchorManager.Instance.DeleteAnchorAsync(cloudAnchor);

                                lock (_dispatchQueue)
                                {
                                    _dispatchQueue.Enqueue(new Action(() =>
                                    {
                                        BroadcastMessage("OnSaveAnchorFailed", anchorObject);
                                    }));
                                }
                            }
                        }

                        IsSaving = false;
                    }
                    catch (Exception ex)
                    {
                        SetStatusMessage("Saving anchor failed, please try again.");
                        Debug.LogError("AnchorManager::OnSetVirtualExplorerAnchor - Failed to save anchor " + ex.Message);
                        Destroy(anchorObject);
                        IsSaving = false;

                        lock (_dispatchQueue)
                        {
                            _dispatchQueue.Enqueue(new Action(() =>
                            {
                                BroadcastMessage("OnSaveAnchorFailed", anchorObject);
                            }));
                        }
                    }
                });
            }));

       
        }
        #endregion

        #region Physical Visualizer
        public void OnSetPhysicalVisualizerAnchor(SpawnData spawnData)
        {
            if (!_enoughDataToCreate || IsPaused)
                return;

            BroadcastMessage("OnSpawnAnchor", spawnData);
        }

        public IEnumerator LoadPhysicalVisualizerAnchors(string anchorSetId, bool ignoreExisting = false)
        {
            var url = string.Format("{0}/{1}/{2}/{3}",
                Globals.ServiceBaseUrl,
                AnchorSetsRoute,
                PhysicalVisualizerRoute,
                anchorSetId);

            Debug.Log("AnchorManager::LoadPhysicalVisualizerAnchors - Calling endpoint: " + url);

            using (var request = UnityWebRequest.Get(url))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log("AnchorManager::LoadPhysicalVisualizerAnchors - Network Error: " + request.error);
                }
                else
                {
                    try
                    {
                       
                       var anchorSet = JsonUtility.FromJson<AnchorSet>(request.downloadHandler.text);

                        if (!ignoreExisting && anchorSet != null && anchorSet.anchors != null && anchorSet.anchors.Any())
                        {
                            _anchors = anchorSet.anchors.ToList();
                            IsLocating = true;
                        }

                        StartAnchorService();
                       
                        lock (_dispatchQueue)
                        {
                            _dispatchQueue.Enqueue(new Action(() =>
                            {
                                BroadcastMessage("OnAnchorsLoaded");
                            }));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("AnchorManager::LoadPhysicalVisualizerAnchors - Failed to load anchors: " + e.ToString());
                    }
                }
            }
        }

        private async Task<bool> SavePhysicalVisualizerAnchor(string anchorSetId, string deviceId, CloudSpatialAnchor anchor)
        {
            var url = string.Format("{0}/{1}/{2}/{3}/{4}",
                Globals.ServiceBaseUrl,
                AnchorSetsRoute,
                PhysicalVisualizerRoute,
                anchorSetId,
                anchor.Identifier);

            return await SaveAnchor(url, deviceId);
        }
        public void ApplyPhysicalVisualizerAnchor(GameObject anchorObject, CloudSpatialAnchor anchor)
        {
            anchorObject.AddARAnchor();

#if WINDOWS_UWP
            anchorObject.GetComponent<WorldAnchor>().SetNativeSpatialAnchorPtr(anchor.LocalAnchor);
#endif
        }

        public void AddPhysicalVisualizerAnchor(GameObject anchorObject, string deviceId)
        {
            if (!_enoughDataToCreate)
                return;

            IsSaving = true;

            var anchorPosition = anchorObject.transform.position;
            StartCoroutine(CreateLocalAnchor(anchorObject, (cloudAnchor) =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        SetStatusMessage("Saving anchor...");

                        cloudAnchor = await AzureSpatialAnchorManager.Instance.StoreAnchorInCloud(cloudAnchor);

                        if (cloudAnchor != null)
                        {
                            Debug.Log("AnchorManager::AddPhysicalVisualizerAnchor - Cloud Anchor created");

                            bool success = await SavePhysicalVisualizerAnchor(AnchorSetManager.Instance.SelectedAnchorSet.id, deviceId, cloudAnchor);

                            if (success)
                            {
                                Debug.Log("AnchorManager::AddPhysicalVisualizerAnchor - Physical Visualizer Anchor created");

                                lock (_cloudAnchors)
                                {
                                    _cloudAnchors.Add(cloudAnchor);
                                }

                                IsSaving = false;
                                SetStatusMessage("Anchor saved");

                                lock (_dispatchQueue)
                                {
                                    _dispatchQueue.Enqueue(new Action(() =>
                                    {
                                        BroadcastMessage("OnAnchorPlaced",
                                            new AnchorPlacedResult()
                                            {
                                                AnchorObject = anchorObject,
                                                AssociatedId = deviceId,
                                                CloudAnchor = cloudAnchor
                                            });
                                    }));
                                }
                            }
                            else
                            {
                                Debug.Log("AnchorManager::AddPhysicalVisualizerAnchor - Failed to create anchor");
                                await AzureSpatialAnchorManager.Instance.DeleteAnchorAsync(cloudAnchor);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SetStatusMessage("Saving anchor failed, please try again.");
                        Debug.LogError("AnchorManager::AddPhysicalVisualizerAnchor - Failed to save anchor " + ex.Message);

                        if (ex.InnerException != null)
                            Debug.LogError("AnchorManager::AddPhysicalVisualizerAnchor - InnerException: " + ex.InnerException.ToString());

                        lock (_dispatchQueue)
                        {
                            _dispatchQueue.Enqueue(new Action(() =>
                            {
                                Destroy(anchorObject);
                            }));
                        }

                        IsSaving = false;
                    }
                });

            }));


        }
        #endregion
    }

    public struct SpawnData
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public struct AnchorLocatedResult
    {
        public SpawnData SpawnData;
        public Anchor Anchor;
        public CloudSpatialAnchor CloudAnchor;
    }

    public struct AnchorPlacedResult
    {
        public GameObject AnchorObject;
        public string AssociatedId;
        public CloudSpatialAnchor CloudAnchor;
    }
}