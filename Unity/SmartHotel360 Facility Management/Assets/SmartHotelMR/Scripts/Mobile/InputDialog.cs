using System;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class InputDialog : MonoBehaviour
    {
        [SerializeField]
        public Text TitleText;

        [SerializeField]
        public Text PlaceholderText;

        [SerializeField]
        public Text TextField;

        private Action<bool, string> Callback { get; set; }

        public string Title { get; private set; }
        public string Placeholder { get; private set; }

        // Update is called once per frame
        void Update()
        {
            if (TitleText != null)
            {
                TitleText.text = Title;
            }

            if (PlaceholderText != null)
            {
                PlaceholderText.text = Placeholder;
            }
        }

        public static void ShowDialog(GameObject prefab, Transform parent, string title, string placeholder, Action<bool, string> callback)
        {
            var instance = GameObject.Instantiate(prefab);
            instance.transform.SetParent(parent, false);
            var dialog = instance.GetComponent<InputDialog>();

            dialog.Title = title;
            dialog.Placeholder = placeholder;
            dialog.Callback = callback;

            instance.SetActive(true);
        }

        public void OnOKButtonClick()
        {
            if (Callback != null)
                Callback(true, TextField.text);

            Destroy(gameObject);
        }

        public void OnCancelButtonClick()
        {
            if (Callback != null)
                Callback(false, TextField.text);

            Destroy(gameObject);
        }
    }
}
