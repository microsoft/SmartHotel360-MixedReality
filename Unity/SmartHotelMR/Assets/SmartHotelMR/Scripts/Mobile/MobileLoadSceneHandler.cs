using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartHotelMR
{
    [RequireComponent(typeof(LoadSceneData))]
    public class MobileLoadSceneHandler : MonoBehaviour
    {
        public void LoadScene()
        {
            Debug.Log(gameObject.name + " : InputDown");

            var sceneData = gameObject.GetComponent<LoadSceneData>();

            if (sceneData != null)
            {
                SmartHotelManager.Instance.LoadScene(sceneData.SceneName, sceneData.IsAdmin);
            }
        }
    }
}