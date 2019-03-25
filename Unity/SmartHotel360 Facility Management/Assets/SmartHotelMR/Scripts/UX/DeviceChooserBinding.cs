using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SmartHotelMR
{
    public class DeviceChooserBinding : ListItemBase, IDeviceMessageTarget
    {
        private bool _isSelected = false;

        private Device _context;
        public Device Context
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
                    textObj.text = "Device " + _context.hardwareId;
            }
        }

        protected override bool IsSelected()
        {
            return _isSelected;
        }

        protected override void OnItemSelected()
        {
            base.OnItemSelected();

            Debug.Log("DeviceChooserBinding::OnItemSelected - " + Context.name);

            this.SendEvent<IDeviceMessageTarget>(null, (x, y) => x.OnDeviceSelected(Context), EventExecutionMethod.BroadcastAll);
        }

        public void OnDeviceSelected(Device device)
        {
            _isSelected = Context == device;
        }
    }
}