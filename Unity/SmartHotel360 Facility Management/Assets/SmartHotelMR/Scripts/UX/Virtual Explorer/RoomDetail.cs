using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SmartHotelMR
{
    public class RoomDetail : MonoBehaviour
    {
        private SpaceBinding _spaceBinding = null;
        private List<SensorData> _sensorData;
        private List<DesiredData> _desiredData;
        private object _dataSyncRoot = new object();

        [Tooltip("Thermostat Indicator")]
        [SerializeField]
        public GameObject ThermostatContentIndicator;

        [Tooltip("Object used for displaying thermostat sensor data")]
        [SerializeField]
        public GameObject ThermostatPanel;

        [Tooltip("Light Indicator")]
        [SerializeField]
        public GameObject LightContentIndicator;

        [Tooltip("Object used for displaying light sensor data")]
        [SerializeField]
        public GameObject LightPanel;

        [Tooltip("Motion Indicator")]
        [SerializeField]
        public GameObject MotionContentIndicator;

        [Tooltip("Object used for displaying motion sensor data")]
        [SerializeField]
        public GameObject MotionPanel;

        void Update()
        {
            if (_spaceBinding == null)
            {
                _spaceBinding = gameObject.GetComponent<SpaceBinding>();
                var dataManager = gameObject.GetComponentInParent<DataManager>();

                if (_spaceBinding == null || dataManager == null)
                {
                    EnableThermostat(false);
                    EnableLight(false);
                    EnableMotion(false);
                    return;
                }

                StartCoroutine(PollForSensorData());
            }
        }

        private IEnumerator PollForSensorData()
        {
            StartCoroutine(LoadSensorData());
            StartCoroutine(LoadDesiredData());
            yield return new WaitForSeconds(3f);
            StartCoroutine(PollForSensorData());
        }

        private IEnumerator LoadSensorData()
        {
            var dataManager = gameObject.GetComponentInParent<DataManager>();

            if (dataManager != null)
                yield return StartCoroutine(dataManager.GetSensorDataForSpace(_spaceBinding.Context.id, OnSensorDataReceived));
        }

        private IEnumerator LoadDesiredData()
        {
            if (_sensorData == null || !_sensorData.Any())
                yield break;

            var dataManager = gameObject.GetComponentInParent<DataManager>();

            if (dataManager != null)
            {
                lock (_dataSyncRoot)
                {
                    yield return StartCoroutine(dataManager.GetDesiredDataForSensors(_sensorData.Select(s => s.sensorId).ToList(), OnDesiredDataReceived));
                }
            }
        }

        public void OnSensorDataReceived(List<SensorData> sensorData)
        {
            lock (_dataSyncRoot)
            {
                _sensorData = sensorData;
            }

            Debug.Log(string.Format("RoomDetail::OnSensorDataReceived - {0}", sensorData.Count));

            StartCoroutine(UpdateSensorData());
        }

        public void OnDesiredDataReceived(List<DesiredData> desiredData)
        {
            lock (_dataSyncRoot)
            {
                _desiredData = desiredData;
            }

            Debug.Log(string.Format("RoomDetail::OnDesiredDataReceived - {0}", desiredData.Count));

            StartCoroutine(UpdateSensorData());
        }

        private IEnumerator UpdateSensorData()
        {
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

                        EnableThermostat(true);
                    }
                    else
                    {
                        Debug.Log("RoomDetail::UpdateSensorData - No thermostat sensor found");
                        EnableThermostat(false);
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

                        EnableLight(true);
                    }
                    else
                    {
                        EnableLight(false);
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

                        EnableMotion(true);
                    }
                    else
                    {
                        EnableMotion(false);
                    }
                }
            }
        }

        private void EnableThermostat(bool enabled)
        {
            if (ThermostatContentIndicator != null)
            {
                ThermostatContentIndicator.SetActive(enabled);
            }

            if (ThermostatPanel != null && !enabled)
            {
                ThermostatPanel.SetActive(enabled);
            }
        }

        private void EnableLight(bool enabled)
        {
            if (LightContentIndicator != null)
            {
                LightContentIndicator.SetActive(enabled);
            }

            if (LightPanel != null && !enabled)
            {
                LightPanel.SetActive(enabled);
            }
        }

        private void EnableMotion(bool enabled)
        {
            if (MotionContentIndicator != null)
            {
                MotionContentIndicator.SetActive(enabled);
            }

            if (MotionPanel != null && !enabled)
            {
                MotionPanel.SetActive(enabled);
            }
        }

        public void OnIndicatorSelected(GameObject indicator)
        {
            Debug.Log("RoomDetail::OnIndicatorSelected - " + indicator.name);

            if (indicator == ThermostatContentIndicator)
            {
                if (ThermostatPanel != null)
                {
                    ThermostatPanel.SetActive(!ThermostatPanel.activeSelf);

                    var sensorBinding = ThermostatPanel.GetComponentInChildren<SensorBinding>();
                    if (sensorBinding.Sensor != null)
                        ExecuteEvents.ExecuteHierarchy<ISensorPanelMessageTarget>(gameObject, null, (x, y) => x.OnSensorPanelToggled(sensorBinding.Sensor.id, ThermostatPanel.activeSelf));
                }
            }
            else if (indicator == LightContentIndicator)
            {
                if (LightPanel != null)
                {
                    LightPanel.SetActive(!LightPanel.activeSelf);

                    var sensorBinding = LightPanel.GetComponentInChildren<SensorBinding>();
                    if (sensorBinding.Sensor != null)
                        ExecuteEvents.ExecuteHierarchy<ISensorPanelMessageTarget>(gameObject, null, (x, y) => x.OnSensorPanelToggled(sensorBinding.Sensor.id, LightPanel.activeSelf));
                }
            }
            else if (indicator == MotionContentIndicator)
            {
                if (MotionPanel != null)
                {
                    MotionPanel.SetActive(!MotionPanel.activeSelf);

                    var sensorBinding = MotionPanel.GetComponentInChildren<SensorBinding>();
                    if (sensorBinding.Sensor != null)
                        ExecuteEvents.ExecuteHierarchy<ISensorPanelMessageTarget>(gameObject, null, (x, y) => x.OnSensorPanelToggled(sensorBinding.Sensor.id, MotionPanel.activeSelf));
                }
            }
        }

        public void UpdateSensorPanelState(Tuple<string, bool> panelState)
        {
            var thermostatSensorBinding = ThermostatPanel.GetComponentInChildren<SensorBinding>();
            var lightSensorBinding = LightPanel.GetComponentInChildren<SensorBinding>();
            var motionSensorBinding = MotionPanel.GetComponentInChildren<SensorBinding>();

            if (thermostatSensorBinding.Sensor != null && thermostatSensorBinding.Sensor.id == panelState.Item1)
            {
                ThermostatPanel.SetActive(panelState.Item2);
            }
            else if (lightSensorBinding.Sensor != null && lightSensorBinding.Sensor.id == panelState.Item1)
            {
                LightPanel.SetActive(panelState.Item2);
            }
            else if (motionSensorBinding.Sensor != null && motionSensorBinding.Sensor.id == panelState.Item1)
            {
                MotionPanel.SetActive(panelState.Item2);
            }
        }
    }
}