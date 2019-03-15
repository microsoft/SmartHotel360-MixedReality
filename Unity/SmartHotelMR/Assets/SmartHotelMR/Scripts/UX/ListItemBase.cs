using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class ListItemBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        [Tooltip("Background Image to use for representing selection state")]
        public Image BackgroundImage;

        [SerializeField]
        [Tooltip("Color used to represent selection state")]
        public Color SelectedColor;

        [SerializeField]
        [Tooltip("Color used when focus is on this item")]
        public Color HighlightColor;

        [SerializeField]
        [Tooltip("Text Object")]
        public Text TextObject;

        [SerializeField]
        [Tooltip("Color used for text on this item")]
        public Color TextColor;

        [SerializeField]
        [Tooltip("Color used for text when focus is on this item")]
        public Color TextHighlightColor;

        private bool TapStarted { get; set; }
        private bool IsFocused { get; set; }

        void Update()
        {
            if (IsSelected())
            {
                BackgroundImage.color = SelectedColor;
                TextObject.color = TextHighlightColor;
            }
            else if (AnchorSetManager.Instance.IsEnabled)
            {
                BackgroundImage.color = IsFocused ? HighlightColor : Color.clear;
                TextObject.color = IsFocused ? TextHighlightColor : TextColor;
            }
            else
            {
                BackgroundImage.color = Color.clear;
                TextObject.color = TextColor;
            }
        }

        protected virtual bool IsSelected()
        {
            return false;
        }

        protected virtual void OnItemSelected()
        {
            IsFocused = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.dragging || eventData.IsScrolling())
                return;

            IsFocused = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TapStarted = false;
            IsFocused = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.dragging || eventData.IsScrolling() || !AnchorSetManager.Instance.IsEnabled)
            {
                TapStarted = false;
                return;
            }

            if (IsFocused && TapStarted)
            {
                OnItemSelected();
            }

            TapStarted = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TapStarted = true;
        }
    }
}