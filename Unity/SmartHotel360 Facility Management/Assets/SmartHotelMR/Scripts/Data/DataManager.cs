using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SmartHotelMR
{
    public class DataManager : MonoBehaviour
    {
        public static readonly string HotelBrandType = "HotelBrand";
        public static readonly string HotelType = "Hotel";
        public static readonly string FloorType = "Floor";
        public static readonly string RoomType = "Room";

        public List<Space> Spaces { get; private set; }
        public List<SensorData> SensorData { get; private set; }
        public List<DesiredData> DesiredData { get; private set; }
        public bool IsInitialized { get; private set; }
        public Dictionary<string, Texture2D> BrandImages { get; private set; }

        public DataManager()
        {
            BrandImages = new Dictionary<string, Texture2D>();
            Spaces = new List<Space>();
            SensorData = new List<SensorData>();
            DesiredData = new List<DesiredData>();
        }

        public IEnumerator Initialize()
        {
            using (var request = UnityWebRequest.Get(Globals.ServiceBaseUrl + "/topology"))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    Spaces = JsonConvert.DeserializeObject<List<Space>>(request.downloadHandler.text);

                    //Load Brand Images
                    yield return StartCoroutine(LoadBrandImages(GetBrands()));
                }

                IsInitialized = true;
                BroadcastMessage("OnDataInitialized");
            }
        }

        protected IEnumerator LoadBrandImages(IEnumerable<Space> brands)
        {
            BrandImages.Clear();

            foreach (var brand in brands)
            {
                string url = string.Format("{0}/brandimages/{1}", Globals.ServiceBaseUrl, brand.id);

                using (var request = UnityWebRequestTexture.GetTexture(url, true))
                {
	                request.AddApiKeyHeader(Globals.ApiKey);
                    yield return request.SendWebRequest();

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.LogError(request.error);
                    }
                    else
                    {
                        var texture = DownloadHandlerTexture.GetContent(request);
                        BrandImages.Add(brand.id, texture);
                    }
                }
            }
        }

        public IEnumerable<Space> GetBrands()
        {
            return Spaces.Where(s => s.type == HotelBrandType).ToList();
        }

        public Texture2D GetBrandImage(string spaceId)
        {
            return BrandImages.ContainsKey(spaceId) ? BrandImages[spaceId] : null;
        }

        public IEnumerable<Space> GetHotels(Space parentSpace)
        {
            if (parentSpace == null)
                return new List<Space>();

            return parentSpace.childSpaces.Where(s => s.type == HotelType).ToList();
        }

        public IEnumerable<Space> GetFloors(Space parentSpace)
        {
            if (parentSpace == null)
                return new List<Space>();

            return parentSpace.childSpaces.Where(s => s.type == FloorType).ToList();
        }

        public IEnumerable<Space> GetRooms(Space parentSpace)
        {
            if (parentSpace == null)
                return new List<Space>();

            return parentSpace.childSpaces.Where(s => s.type == RoomType).ToList();
        }

        public Space GetSpaceById(string spaceId)
        {
            var space = Spaces.FirstOrDefault(s => s.id == spaceId);

            if (space == null)
                space = Spaces.Flatten<Space>(s => s.childSpaces).FirstOrDefault(s => s.id == spaceId);

            return space;
        }

        public Device GetDeviceBySpaceAndId(string spaceId, string deviceId)
        {
            var space = GetSpaceById(spaceId);

            if (space != null)
            {
                return space.devices == null ? null : space.devices.FirstOrDefault(d => d.id == deviceId);
            }

            return null;
        }

        public Device GetDeviceById(string deviceId)
        {
            var spaces = Spaces.Flatten<Space>(s => s.childSpaces);
            var devices = spaces.Where(s => s.devices != null && s.devices.Any()).SelectMany(s => s.devices);
            return devices.FirstOrDefault(d => d != null && d.id == deviceId);
        }

        public IEnumerator GetSensorDataForSpace(string spaceId, Action<List<SensorData>> callback)
        {
            using (var request = UnityWebRequest.Get(string.Format("{0}/sensordata/{1}", Globals.ServiceBaseUrl, "?roomIds=" + spaceId)))
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
                        var sensorData = JsonConvert.DeserializeObject<List<SensorData>>(request.downloadHandler.text);
                        callback(sensorData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }
            }
        }

        public IEnumerator GetDesiredDataForSensors(IEnumerable<string> sensorIds, Action<List<DesiredData>> callback)
        {
            var url = Globals.ServiceBaseUrl + @"/desireddata";

            for (int i = 0; i < sensorIds.Count(); i++)
            {
                url += (i == 0 ? "?" : "&") + "sensorIds=" + sensorIds.ElementAt(i);
            }

            Debug.Log("DataManager::GetDesiredDataForSensors - Calling url " + url);

            using (var request = UnityWebRequest.Get(url))
            {
                request.AddApiKeyHeader(Globals.ApiKey);

                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError("DataManager::GetDesiredDataForSensors - " + request.error);
                }
                else
                {
                    try
                    {
                        var desiredData = JsonConvert.DeserializeObject<List<DesiredData>>(request.downloadHandler.text);
                        callback(desiredData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("DataManager::GetDesiredDataForSensors - " + e.ToString());
                    }
                }
            }
        }
    }
}
