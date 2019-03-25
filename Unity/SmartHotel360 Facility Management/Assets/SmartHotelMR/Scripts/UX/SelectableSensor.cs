using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartHotelMR
{
    public class SelectableSensor : MonoBehaviour
    {
        [SerializeField]
        public Color NormalColor;

        [SerializeField]
        public Color SelectedColor;

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                UpdateSelectionState();
            }
        }

        private void UpdateSelectionState()
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();

            if (renderers == null)
                return;

            foreach (var renderer in renderers)
            {
                renderer.material.color = IsSelected ? SelectedColor : NormalColor;
            }
        }
    }
}