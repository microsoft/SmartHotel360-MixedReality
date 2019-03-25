using System;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class SensorBinding : MonoBehaviour
    {
        [Tooltip("Current Data Text Object")]
        [SerializeField]
        public Text SensorDataText;

        [Tooltip("Desired Data Text Object")]
        [SerializeField]
        public Text DesiredDataText;

        private SensorData _sensorData;
        public SensorData Sensor
        {
            get
            {
                return _sensorData;
            }
            set
            {
                if (_sensorData != value)
                {
                    _sensorData = value;
                    UpdateSensorLabel();
                    UpdateDesiredLabel();
                }
            }
        }

        private DesiredData _desiredData;
        public DesiredData Desired
        {
            get
            {
                return _desiredData;
            }
            set
            {
                if (_desiredData != value)
                {
                    _desiredData = value;
                    UpdateDesiredLabel();
                }
            }
        }

        private void UpdateDesiredLabel()
        {
            if (DesiredDataText != null)
            {
                DesiredDataText.text = GetDesiredDataString();
            }
        }

        private void UpdateSensorLabel()
        {
            if (SensorDataText != null)
            {
                SensorDataText.text = GetSensorDataString();
            }
        }

        private string GetSensorDataString()
        {
            string sensorText = Sensor == null ? "N/A" : Sensor.sensorReading;

            if (Sensor != null)
            {
                if (Sensor.sensorDataType.ToLower() == "temperature")
                    sensorText += "°";
                else if (Sensor.sensorDataType.ToLower() == "light")
                    sensorText = string.Format("{0}%", Convert.ToDouble(Sensor.sensorReading) * 100);
                else if (Sensor.sensorDataType.ToLower() == "motion")
                    sensorText = Convert.ToBoolean(Sensor.sensorReading) ? "Occupied" : "Unoccupied";

                //Debug.Log(string.Format("SensorBinding::Sensor {0} value = {1}", Sensor.sensorDataType, sensorText));
            }
            else
            {
                Debug.Log(string.Format("SensorBinding::Sensor {0} value = {1}", SensorDataText.gameObject.name, sensorText));
            }

            return sensorText;
        }

        private string GetDesiredDataString()
        {
            var desiredText = Desired == null ? GetSensorDataString() : Desired.desiredValue;

            if (Sensor != null && Desired != null)
            {
                if (Sensor.sensorDataType.ToLower() == "temperature")
                    desiredText += "°";
                else if (Sensor.sensorDataType.ToLower() == "light")
                    desiredText = string.Format("{0}%", Convert.ToDouble(Desired.desiredValue) * 100);

                //Debug.Log(string.Format("SensorBinding::Desired {0} value = {1}", Sensor.sensorDataType, desiredText));
            }
            else
            {
                Debug.Log(string.Format("SensorBinding::Desired {0} value = {1}", DesiredDataText.gameObject.name, desiredText));
            }

            return desiredText;
        }
    }
}