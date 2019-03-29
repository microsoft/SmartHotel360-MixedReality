using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SmartHotelMR
{
    [RequireComponent(typeof(DataManager))]
    public class StateManager : MonoBehaviour, ISpaceMessageTarget, ISensorPanelMessageTarget
    {
        private DataManager _dataManager;

        public Space SelectedSpace { get; private set; }
        public State CurrentState { get; private set; }

        void Start()
        {
            CurrentState = new State();
            _dataManager = gameObject.GetComponent<DataManager>();

#if !UNITY_EDITOR
            StartCoroutine(PollForState());
#endif
        }

        public IEnumerator SetSelectedSpace(Space space)
        {
            if (SelectedSpace == space)
                yield break;

            if (space != null)
                Debug.Log(string.Format("StateManager::SetSelectedSpace - Space {0}", space.name));

            SelectedSpace = space;
            CurrentState.currentSelectedSpace = space == null ? null : space.id;

            yield return StartCoroutine(SetCurrentState());

            BroadcastMessage("SelectedSpaceChanged", space == null ? new Space() : space);
        }

        public IEnumerator SetSensorPanelToggled(string sensorId, bool state)
        {
            if (string.IsNullOrEmpty(sensorId))
                yield break;

            CurrentState.toggledSensorPanels[sensorId] = state;

            yield return StartCoroutine(SetCurrentState());
        }

        public void OnSpaceSelected(Space space)
        {
            StartCoroutine(SetSelectedSpace(space));
        }

        public void OnSensorPanelToggled(string sensorId, bool state)
        {
            StartCoroutine(SetSensorPanelToggled(sensorId, state));
        }

        protected IEnumerator PollForState()
        {
            StartCoroutine(GetCurrentState());
            yield return new WaitForSeconds(1f);
            StartCoroutine(PollForState());
        }

        protected IEnumerator GetCurrentState()
        {
            if (AnchorSetManager.Instance == null || AnchorSetManager.Instance.SelectedAnchorSet == null)
                yield break;

            var url = string.Format("{0}/sharedstate/{1}", Globals.ServiceBaseUrl, AnchorSetManager.Instance.SelectedAnchorSet.id);

            using (var request = UnityWebRequest.Get(url))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    try
                    {
                        if (request.responseCode == 200)
                        {
                            var state = JsonUtility.FromJson<State>(request.downloadHandler.text);

                            if (state.currentSelectedSpace != CurrentState.currentSelectedSpace)
                            {
                                if (_dataManager == null)
                                    throw new NullReferenceException("DataManager is null, please verify it is attached to this object.");

                                var space = _dataManager.GetSpaceById(state.currentSelectedSpace);

                                SelectedSpace = space;

                                BroadcastMessage("SelectedSpaceChanged", space == null ? new Space() : space);
                            }

                            if (SelectedSpace == null && !string.IsNullOrEmpty(state.currentSelectedSpace))
                                SelectedSpace = _dataManager.GetSpaceById(state.currentSelectedSpace);

                            CurrentState = state;

                            foreach (var key in state.toggledSensorPanels.Keys)
                            {
                                BroadcastMessage("UpdateSensorPanelState", new Tuple<string, bool>(key, state.toggledSensorPanels[key]));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        protected IEnumerator SetCurrentState()
        {
            if (AnchorSetManager.Instance == null || AnchorSetManager.Instance.SelectedAnchorSet == null)
                yield break;

            var url = string.Format("{0}/sharedstate/{1}", Globals.ServiceBaseUrl, AnchorSetManager.Instance.SelectedAnchorSet.id);

            using (var request = new UnityWebRequest(url))
            {
                request.method = UnityWebRequest.kHttpVerbPUT;
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CurrentState)));
                request.SetRequestHeader("Content-Type", "application/json");
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);
                }
            }
        }
    }
}
