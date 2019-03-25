using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class AnchorSetBinding : ListItemBase
    {
        private AnchorSet _context;
        public AnchorSet Context
        {
            get { return _context; }
            set
            {
                _context = value;
                UpdateBinding();
            }
        }
        
        void Start()
        {
            UpdateBinding();
        }

        protected override bool IsSelected()
        {
            if (AnchorSetManager.Instance != null)
            {
                return AnchorSetManager.Instance.SelectedAnchorSet == Context && AnchorSetManager.Instance.IsEnabled;
            }

            return false;
        }

        private void UpdateBinding()
        {
            if (_context != null)
            {
                var textObj = gameObject.GetComponentInChildren<Text>();

                if (textObj != null)
                    textObj.text = _context.name;
            }
        }

        protected override void OnItemSelected()
        {
            base.OnItemSelected();

            Debug.Log("AnchorSetBinding::OnAnchorSelected - " + Context.name);

            if (AnchorSetManager.Instance != null)
            {                
                AnchorSetManager.Instance.SelectedAnchorSet = Context;
            }
        }
    }
}
