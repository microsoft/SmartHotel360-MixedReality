using HoloToolkit.Unity.InputModule;
using SmartHotelMR;
using UnityEngine;

public class HololensSpeechHandler : MonoBehaviour, ISpeechHandler
{
    [SerializeField]
    public string MainMenuSceneName;

    void Start()
    {
        InputManager.Instance.AddGlobalListener(this.gameObject);
    }

    void OnDestroy()
    {
        InputManager.Instance.RemoveGlobalListener(this.gameObject);
    }

    public void OnSpeechKeywordRecognized(SpeechEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(eventData.RecognizedText)
            && eventData.RecognizedText.ToLower() == "show menu")
        {
            SmartHotelManager.Instance.LoadScene(MainMenuSceneName);
        }

        if (!string.IsNullOrWhiteSpace(eventData.RecognizedText)
            && eventData.RecognizedText.ToLower() == "exit admin")
        {
            BroadcastMessage("OnExitAdminMode");
        }

        if (!string.IsNullOrWhiteSpace(eventData.RecognizedText)
            && eventData.RecognizedText.ToLower() == "placement mode")
        {
            BroadcastMessage("OnSetPlacementMode", true);
        }

        if (!string.IsNullOrWhiteSpace(eventData.RecognizedText)
            && eventData.RecognizedText.ToLower() == "selection mode")
        {
            BroadcastMessage("OnSetDeleteMode", true);
        }
    }
}
