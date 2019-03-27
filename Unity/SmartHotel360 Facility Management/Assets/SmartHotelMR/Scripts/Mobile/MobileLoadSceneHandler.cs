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
                string sceneName = "";

	            if (!sceneData.SharedScene)
	            {
#if UNITY_IOS
                sceneName = "Ios" + sceneData.SceneName;
#endif
#if UNITY_ANDROID
		            sceneName = "Android" + sceneData.SceneName;
#endif
	            }
	            else
	            {
		            sceneName = sceneData.SceneName;
	            }

	            SmartHotelManager.Instance.LoadScene(sceneName, sceneData.IsAdmin);
            }
        }
    }
}