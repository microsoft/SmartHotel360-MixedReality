using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class DeviceBinding : MonoBehaviour
    {
        private DataManager _dataManager;
        private List<SensorData> _sensorData;
        private List<DesiredData> _desiredData;
        private object _dataSyncRoot = new object();
        private bool _continuePolling = true;

        [Tooltip("Object used for displaying thermostat sensor data")]
        [SerializeField]
        public GameObject ThermostatPanel;

        [Tooltip("Object used for displaying light sensor data")]
        [SerializeField]
        public GameObject LightPanel;

        [Tooltip("Object used for displaying motion sensor data")]
        [SerializeField]
        public GameObject MotionPanel;

        [Tooltip("Object used for displaying space name")]
        [SerializeField]
        public Text TitleObject;

        [SerializeField]
        [Tooltip("Content to show when loading")]
        public GameObject LoadingIndicator;

        public string DeviceId { get; set; }

        private Device _device;
        public Device Device
        {
            get
            {
                return _device;
            }
            private set
            {
                _device = value;

                if (gameObject.activeSelf)
                    StartCoroutine(UpdateTitle());
            }
        }

        private void OnEnable()
        {
            _continuePolling = true;
            StartCoroutine(PollForSensorData());
        }

        private void OnDisable()
        {
            _continuePolling = false;
        }

        private void Update()
        {
            if (_dataManager == null)
            {
                _dataManager = gameObject.GetComponentInParent<DataManager>();

                if (_dataManager == null)
                    Debug.LogError("DeviceBinding::Update - Can't find DataManager");
            }
            else if (Device == null && !string.IsNullOrEmpty(DeviceId))
            {
                Device = _dataManager.GetDeviceById(DeviceId);
            }
        }

        private IEnumerator PollForSensorData()
        {
            if (!_continuePolling)
                yield break;

            StartCoroutine(UpdateTitle());
            StartCoroutine(LoadSensorData());
            StartCoroutine(LoadDesiredData());
            yield return new WaitForSeconds(3f);
            StartCoroutine(PollForSensorData());
        }

        private IEnumerator LoadSensorData()
        {
            if (_dataManager != null && Device != null)
            {
                yield return StartCoroutine(_dataManager.GetSensorDataForSpace(Device.spaceId, OnSensorDataReceived));
            }
            else
            {
                Debug.LogWarning("DeviceBinding::LoadSensorData - DataManager or Device is null");
            }
        }

        private IEnumerator LoadDesiredData()
        {
            if (_dataManager != null && Device != null)
            {
                var sensors = Device.sensors.Select(s => s.id).ToList();

                yield return StartCoroutine(_dataManager.GetDesiredDataForSensors(sensors, OnDesiredDataReceived));
            }
            else
            {
                Debug.LogWarning("DeviceBinding::LoadDesiredData - DataManager or Device is null");
            }
        }

        public void OnSensorDataReceived(List<SensorData> sensorData)
        {
            lock (_dataSyncRoot)
            {
                _sensorData = sensorData;
            }

            Debug.Log(string.Format("DeviceBinding::OnSensorDataReceived - {0}", sensorData.Count));

            StartCoroutine(UpdateSensorData());
        }

        public void OnDesiredDataReceived(List<DesiredData> desiredData)
        {
            lock (_dataSyncRoot)
            {
                _desiredData = desiredData;
            }

            Debug.Log(string.Format("DeviceBinding::OnDesiredDataReceived - {0}", desiredData.Count));

            StartCoroutine(UpdateSensorData());
        }

        private IEnumerator UpdateSensorData()
        {
            yield return StartCoroutine(HideLoadingIndicator());

            lock (_dataSyncRoot)
            {
                if (_sensorData == null || !_sensorData.Any())
                    yield break;

                if (ThermostatPanel != null)
                {
                    var sensorBinding = ThermostatPanel.GetComponentInChildren<SensorBinding>();

                    sensorBinding.Sensor = _sensorData.FirstOrDefault(s => s.sensorDataType.ToLower() == "temperature");

                    if (sensorBinding.Sensor != null)
                    {
                        if (_desiredData != null)
                            sensorBinding.Desired = _desiredData.FirstOrDefault(d => d.sensorId == sensorBinding.Sensor.sensorId);
                    }
                    else
                    {
                        Debug.LogWarning("DeviceBinding::UpdateSensorData - No thermostat sensor found");
                    }
                }

                yield return new WaitForEndOfFrame();

                if (LightPanel != null)
                {
                    var sensorBinding = LightPanel.GetComponentInChildren<SensorBinding>();

                    sensorBinding.Sensor = _sensorData.FirstOrDefault(s => s.sensorDataType.ToLower() == "light");

                    if (sensorBinding.Sensor != null)
                    {
                        if (_desiredData != null)
                            sensorBinding.Desired = _desiredData.FirstOrDefault(d => d.sensorId == sensorBinding.Sensor.sensorId);
                    }
                    else
                    {
                        Debug.LogWarning("DeviceBinding::UpdateSensorData - No light sensor found");
                    }
                }

                yield return new WaitForEndOfFrame();

                if (MotionPanel != null)
                {
                    var sensorBinding = MotionPanel.GetComponentInChildren<SensorBinding>();

                    sensorBinding.Sensor = _sensorData.FirstOrDefault(s => s.sensorDataType.ToLower() == "motion");

                    if (sensorBinding.Sensor != null)
                    {
                        if (_desiredData != null)
                            sensorBinding.Desired = _desiredData.FirstOrDefault(d => d.sensorId == sensorBinding.Sensor.sensorId);
                    }
                    else
                    {
                        Debug.LogWarning("DeviceBinding::UpdateSensorData - No motion sensor found");
                    }
                }
            }
        }

        private IEnumerator HideLoadingIndicator()
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.SetActive(false);
            }

            ThermostatPanel.SetActive(true);
            LightPanel.SetActive(true);
            MotionPanel.SetActive(true);

            yield return new WaitForEndOfFrame();
        }

        private IEnumerator UpdateTitle()
        {
            if (_dataManager == null || Device == null)
                yield break;

            var space = _dataManager.GetSpaceById(Device.spaceId);

            if (space != null)
                TitleObject.text = space.friendlyName;
        }
    }
}