using System;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField]
        public Text MessageText;

        private Action<bool> Callback { get; set; }
        public string Message { get; private set; }

        // Update is called once per frame
        void Update()
        {
            if (MessageText != null)
            {
                MessageText.text = Message;
            }
        }

        public static void ShowDialog(GameObject prefab, Transform parent, string message, Action<bool> callback)
        {
            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(parent, false);
            var dialog = instance.GetComponent<ConfirmationDialog>();

            dialog.Message = message;
            dialog.Callback = callback;

            instance.SetActive(true);
        }

        public void OnYesButtonClick()
        {
            if (Callback != null)
                Callback(true);

            Destroy(gameObject);
        }

        public void OnNoButtonClick()
        {
            if (Callback != null)
                Callback(false);

            Destroy(gameObject);
        }
    }
}
