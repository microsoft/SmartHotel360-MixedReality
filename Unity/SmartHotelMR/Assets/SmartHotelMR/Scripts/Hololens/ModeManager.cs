using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using SmartHotelMR;
using UnityEngine;

public class ModeManager : InteractionReceiver
{
    protected override void InputDown(GameObject obj, InputEventData eventData)
    {
        Debug.Log(obj.name + " : InputDown");

        var sceneData = obj.GetComponent<LoadSceneData>();

        if (sceneData != null)
        {
            SmartHotelManager.Instance.LoadScene(sceneData.SceneName, sceneData.IsAdmin);
        }
    }
}
