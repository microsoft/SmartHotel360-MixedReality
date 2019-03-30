using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class SpaceChooserBinding : ListItemBase
    {
        private Space _context;
        public Space Context
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

            Debug.Log("SpaceChooserBinding::OnItemSelected - " + Context.name);

            this.SendEvent<ISpaceMessageTarget>(null, (x, y) => x.OnSpaceSelected(Context), EventExecutionMethod.BroadcastAll);
        }
    }
}