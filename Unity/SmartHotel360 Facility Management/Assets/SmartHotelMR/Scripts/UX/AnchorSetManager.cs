using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace SmartHotelMR
{
    public class AnchorSetManager : Singleton<AnchorSetManager>
    {
        private const string AnchorSetsUrl = "/anchorsets";

        [SerializeField]
        public string MainMenuSceneName;

        [SerializeField]
        [Tooltip("Prefab to use for each AnchorSet item")]
        public GameObject ItemPrefab;

        [SerializeField]
        [Tooltip("Object to add AnchorSetItems to")]
        public GameObject ContentArea;

        [SerializeField]
        [Tooltip("Content to show when loading")]
        public GameObject LoadingIndicator;

        public AnchorSet SelectedAnchorSet { get; set; }
        public bool IsEnabled { get; set; }
       
        void Start()
        {
            IsEnabled = true;
            StartCoroutine(LoadAnchorSets());
        }

        private IEnumerator LoadAnchorSets()
        {
            using (var request = UnityWebRequest.Get(Globals.ServiceBaseUrl + AnchorSetsUrl))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    var anchors = JsonUtility.FromJson<AnchorSetWrapper>("{\"values\":" + request.downloadHandler.text + "}")?.values;

                    foreach (var set in anchors.OrderBy(a => a.name))
                    {
                        var obj = GameObject.Instantiate(ItemPrefab);
                        var binding = obj.GetComponent<AnchorSetBinding>();
                        binding.Context = set;

                        obj.transform.SetParent(ContentArea.transform, false);

                        yield return new WaitForEndOfFrame();
                    }

                    if (LoadingIndicator != null)
                        LoadingIndicator.SetActive(false);
                }
            }
        }

        public IEnumerator AddNewAnchorSet(string anchorSetName)
        {
            IsEnabled = true;

            using (var request = new UnityWebRequest(Globals.ServiceBaseUrl + AnchorSetsUrl))
            {
                request.method = UnityWebRequest.kHttpVerbPOST;
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(anchorSetName)));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                    yield break;
                }

                SelectedAnchorSet = JsonUtility.FromJson<AnchorSet>(request.downloadHandler.text);
                OnLoadAnchorSet();
            }
        }

        public IEnumerator DeleteAnchorSet(AnchorSet anchorSet)
        {
            using (var request = UnityWebRequest.Delete(string.Format("{0}/{1}", Globals.ServiceBaseUrl + AnchorSetsUrl, anchorSet.id)))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.Log(request.error);
                    yield break;
                }

                for (int i = 0; i < ContentArea.transform.childCount; i++)
                {
                    var child = ContentArea.transform.GetChild(i);
                    var binding = child.GetComponent<AnchorSetBinding>();

                    if (binding.Context == anchorSet)
                    {
                        Destroy(child.gameObject);
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }

                SelectedAnchorSet = null;
            }
        }

        public void OnNewAnchorSet()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;

            Debug.Log("AnchorSetManager::OnNewAnchorSet");
            SendMessageUpwards("HandleNewAnchorSet");
        }

        public void OnLoadAnchorSet()
        {
            if (SelectedAnchorSet == null || !IsEnabled)
                return;

            Debug.Log("AnchorSetManager::OnLoadAnchorSet");
            SmartHotelManager.Instance.LoadScene(MainMenuSceneName);
        }

        public void OnDeleteAnchorSet()
        {
            if (SelectedAnchorSet == null || !IsEnabled)
                return;

            IsEnabled = false;

            Debug.Log("AnchorSetManager::OnDeleteAnchorSet");
            SendMessageUpwards("HandleDeleteAnchorSet", SelectedAnchorSet);
        }
    }
}