using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    [RequireComponent(typeof(AnchorManager))]
    [RequireComponent(typeof(DataManager))]
    [RequireComponent(typeof(StateManager))]
    public class VirtualExplorerManagerBase : MonoBehaviour
    {
        protected bool _isInitialized;
        protected AnchorManager _anchorManager;
        protected DataManager _dataManager;
        protected StateManager _stateManager;

        [Tooltip("Scene to load when exiting Admin Mode")]
        public string MainMenuSceneName;

        [Tooltip("Content to show when in Admin Mode")]
        public GameObject AdminContent;
        [Tooltip("Content to show when in User Mode and no anchor is set")]
        public GameObject NoAnchorContent;
        [Tooltip("Breadcrumb object used to show parent space name")]
        public TextMesh Breadcrumb;


        [Tooltip("Prefab to use for representing Hotel Brand Space(s)")]
        public GameObject HotelBrandPrefab;
        [Tooltip("Prefab to use for representing Hotel Space(s)")]
        public GameObject HotelPrefab;
        [Tooltip("Prefab to use for representing Floor Space(s)")]
        public GameObject FloorPrefab;
        [Tooltip("Prefab to use for representing Room Space(s)")]
        public GameObject RoomPrefab;
        [Tooltip("Prefab to use for representing a single Room")]
        public GameObject RoomDetailPrefab;

        [Tooltip("CollectionLayout to use for displaying Hotel Brand Space(s)")]
        public GameObject HotelBrandCollection;
        [Tooltip("CollectionLayout to use for displaying Hotel Space(s)")]
        public GameObject HotelCollection;
        [Tooltip("CollectionLayout to use for displaying Floor Space(s)")]
        public GameObject FloorCollection;
        [Tooltip("CollectionLayout to use for displaying Room Space(s)")]
        public GameObject RoomCollection;
        [Tooltip("CollectionLayout to use for displaying a single Room")]
        public GameObject RoomDetailCollection;

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
            _anchorManager = GetComponent<AnchorManager>();
            _dataManager = GetComponent<DataManager>();
            _stateManager = GetComponent<StateManager>();

            ShowLoadingIndicator("Loading Anchors...");
        }

        public virtual void Update()
        {
#if UNITY_EDITOR
            if (SmartHotelManager.Instance == null)
                return;
#endif
            if (!_isInitialized)
            {
                StartCoroutine(_anchorManager.LoadVirtualExplorerAnchor(AnchorSetManager.Instance.SelectedAnchorSet.id,
                                                                        SmartHotelManager.Instance.IsAdminMode));
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

        public virtual void SelectedSpaceChanged(Space space)
        {
            Debug.Log("VirtualExplorerManagerBase::SelectedSpaceChanged");

            HideAllCollections();

            GameObject collection = null;
            GameObject prefab = null;
            IEnumerable<Space> spaces = null;

            if (space == null || string.IsNullOrWhiteSpace(space.type))
            {
                collection = HotelBrandCollection;
                prefab = HotelBrandPrefab;
                spaces = _dataManager.GetBrands().OrderBy(s => s.name).ToList();
            }
            else if (space.type == DataManager.HotelBrandType)
            {
                collection = HotelCollection;
                prefab = HotelPrefab;
                spaces = space.childSpaces.OrderBy(s => s.name).ToList();
            }
            else if (space.type == DataManager.HotelType)
            {
                collection = FloorCollection;
                prefab = FloorPrefab;
                spaces = space.childSpaces.OrderByDescending(s => s.name).ToList();
            }
            else if (space.type == DataManager.FloorType)
            {
                collection = RoomCollection;
                prefab = RoomPrefab;
                spaces = space.childSpaces.OrderBy(s => s.name).ToList();
            }
            else if (space.type == DataManager.RoomType)
            {
                var child = new List<Space>();
                child.Add(space);

                spaces = child;
                prefab = RoomDetailPrefab;
                collection = RoomDetailCollection;
            }
            else
            {
                return;
            }

            if (collection != null && prefab != null && spaces != null && spaces.Count() > 0)
            {
                LoadObjectCollection(spaces, collection, prefab);
            }

            StartCoroutine(EnableBackButton(collection, space != null && !string.IsNullOrWhiteSpace(space.type)));
            StartCoroutine(ShowBreadcrumb(collection, space, space != null && !string.IsNullOrWhiteSpace(space.type)));
        }

        protected virtual IEnumerator EnableBackButton(GameObject collection, bool enabled)
        {
            throw new NotImplementedException();
        }

        public virtual void OnDataInitialized()
        {
            if (_anchorManager.HasAnchors())
            {
                var space = _dataManager.GetSpaceById(_stateManager.CurrentState.currentSelectedSpace);
                SelectedSpaceChanged(space);
                StartCoroutine(HideLoadingIndicator());
            }
        }

        public virtual void OnAnchorsLoaded()
        {
            StartCoroutine(HideLoadingIndicator());

            if (NoAnchorContent != null)
                NoAnchorContent.SetActive(!_anchorManager.IsLocating && !SmartHotelManager.Instance.IsAdminMode);
        }

        public virtual void OnAnchorLocated(AnchorLocatedResult result)
        {
            Debug.Log(string.Format("VirtualExplorerManagerBase::OnAnchorLocated - Position: {0}, Rotation: {1}",
                string.Format("({0}, {1}, {2})", result.SpawnData.Position.x, result.SpawnData.Position.y, result.SpawnData.Position.z),
                string.Format("({0}, {1}, {2})", result.SpawnData.Rotation.x, result.SpawnData.Rotation.y, result.SpawnData.Rotation.z)));

            gameObject.transform.position = result.SpawnData.Position;
            gameObject.transform.rotation = result.SpawnData.Rotation;

            _anchorManager.ApplyVirtualExplorerAnchor(gameObject);
        }

        public virtual void OnAnchorsLocated()
        {
            if (!SmartHotelManager.Instance.IsAdminMode)
            {
                if (StatusObject != null)
                {
                    StatusObject.gameObject.SetActive(false);
                }
            }

            if (_anchorManager.HasAnchors() && !SmartHotelManager.Instance.IsAdminMode)
            {
                ShowLoadingIndicator("Loading Spaces...");
                StartCoroutine(_dataManager.Initialize());
            }
            else
            {
                if (NoAnchorContent != null)
                    NoAnchorContent.SetActive(!SmartHotelManager.Instance.IsAdminMode);
            }
        }

        public virtual void OnAnchorPlaced(AnchorPlacedResult result)
        {
            _anchorManager.StopAnchorService();
            SmartHotelManager.Instance.IsAdminMode = false;

            StartCoroutine(PlaceAnchor(result.AnchorObject));
        }

        protected virtual IEnumerator PlaceAnchor(GameObject anchor)
        {
            yield return new WaitForEndOfFrame();

            _anchorManager.ApplyVirtualExplorerAnchor(gameObject);

            yield return new WaitForEndOfFrame();

            if (_dataManager.IsInitialized)
            {
                LoadObjectCollection(_dataManager.GetBrands().OrderBy(b => b.name).ToList(), HotelBrandCollection, HotelBrandPrefab);
            }
            else
            {
                ShowLoadingIndicator("Loading Spaces...");

                yield return new WaitForEndOfFrame();

                StartCoroutine(_dataManager.Initialize());
            }
        }

        protected void HideAllCollections()
        {
            if (HotelBrandCollection != null)
            {
                EmptyCollection(HotelBrandCollection);
                HotelBrandCollection.SetActive(false);
            }

            if (HotelCollection != null)
            {
                EmptyCollection(HotelCollection);
                HotelCollection.SetActive(false);
            }

            if (FloorCollection != null)
            {
                EmptyCollection(FloorCollection);
                FloorCollection.SetActive(false);
            }

            if (RoomCollection != null)
            {
                EmptyCollection(RoomCollection);
                RoomCollection.SetActive(false);
            }

            if (RoomDetailCollection != null)
            {
                EmptyCollection(RoomDetailCollection);
                RoomDetailCollection.SetActive(false);
            }
        }

        protected void EmptyCollection(GameObject collection)
        {
            // Remove existing children
            while (collection.transform.childCount > 0)
            {
                Transform child = collection.transform.GetChild(0);
                child.parent = null;
                DestroyImmediate(child.gameObject);
            }
        }

        protected void LoadObjectCollection(IEnumerable<Space> spaces, GameObject collection, GameObject prefab)
        {
            EmptyCollection(collection);

            // Generate new space children
            foreach (var space in spaces)
            {
                GameObject spaceGameObject = GameObject.Instantiate(prefab);

                var context = spaceGameObject.GetComponent<SpaceBinding>();
                context.Context = space;

                AddInteractionComponents(space, spaceGameObject);

                spaceGameObject.transform.SetParent(collection.transform, false);
            }

            collection.GetComponent<CollectionLayout>().UpdateCollection();
            collection.SetActive(true);
        }

        protected virtual void AddInteractionComponents(Space space, GameObject instance)
        {
            throw new NotImplementedException();
        }

        public virtual void SetAnchorStatusMessage(string message)
        {
            if (StatusObject != null)
                StatusObject.text = message;
        }

        protected Bounds GetCollectionBounds(GameObject collection)
        {
            var colliders = collection.GetComponentsInChildren<Collider>();

            if (colliders != null && colliders.Any())
            {
                Bounds collectiveBounds = colliders[0].bounds;

                foreach (var collider in colliders.Skip(1))
                {
                    collectiveBounds = collectiveBounds.ExpandToContain(collider.bounds);
                }

                return collectiveBounds;
            }

            return new Bounds(Vector3.zero, Vector3.zero);
        }

        protected virtual IEnumerator ShowBreadcrumb(GameObject collection, Space space, bool enabled)
        {
            if (Breadcrumb != null && collection != null)
            {
                if (space != null)
                {
                    if (space.type == DataManager.RoomType)
                    {
                        Breadcrumb.gameObject.SetActive(false);
                        yield break;
                    }

                    Breadcrumb.text = space.friendlyName;
                    yield return new WaitForEndOfFrame();
                }

                var renderer = Breadcrumb.gameObject.GetComponent<MeshRenderer>();
                var collectionBounds = GetCollectionBounds(collection);

                float x = collectionBounds.center.x;
                float y = collectionBounds.max.y + (renderer.bounds.size.y * 2f);
                float z = collectionBounds.center.z;

                Breadcrumb.gameObject.transform.position = new Vector3(x, y, z);
                Breadcrumb.gameObject.SetActive(enabled);
            }
        }

        public virtual void OnExitAdminMode()
        {
            SmartHotelManager.Instance.LoadScene(MainMenuSceneName);
        }
    }
}