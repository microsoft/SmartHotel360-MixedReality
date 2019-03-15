using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SmartHotelMR
{
    [RequireComponent(typeof(DataManager))]
    public class DigitalTwinsChooser : MonoBehaviour, ISpaceMessageTarget, IDeviceMessageTarget
    {
        private DataManager _dataManager;
        private GameObject _currentAnchor;
        private Space _currentSpace = null;
        private Device _selectedDevice = null;

        [SerializeField]
        [Tooltip("Root panel for the chooser")]
        public GameObject ChooserPanel;

        [SerializeField]
        [Tooltip("Object used to display currently selected Space name")]
        public Text BreadcrumbLabel;

        [SerializeField]
        [Tooltip("Prefab to use for Spaces in the list")]
        public GameObject SpaceItemPrefab;

        [SerializeField]
        [Tooltip("Prefab to use for Devices in the list")]
        public GameObject DeviceItemPrefab;

        [SerializeField]
        [Tooltip("Game object to add items to as children")]
        public GameObject ContentArea;

        [SerializeField]
        [Tooltip("GameObject represeting the Back button")]
        public GameObject BackButton;

        [SerializeField]
        [Tooltip("GameObject represeting the Select button")]
        public Button SelectButton;

        void Start()
        {
            _dataManager = GetComponent<DataManager>();
        }

        public void OnSelect()
        {
            if (ChooserPanel != null)
                ChooserPanel.SetActive(false);

            BroadcastMessage("OnChooserClosed", new ChooserResult() { Cancelled = false, SelectedDevice = _selectedDevice, Anchor = _currentAnchor });
        }

        public void OnCancel()
        {
            StartCoroutine(Hide());
        }

        public IEnumerator Hide()
        {
            if (ChooserPanel != null)
                ChooserPanel.SetActive(false);

            Destroy(_currentAnchor);

            yield return new WaitForEndOfFrame();

            BroadcastMessage("OnChooserClosed", new ChooserResult() { Cancelled = true });
        }

        public void OnNavigateBack()
        {
            var space = _dataManager.GetSpaceById(_currentSpace.parentSpaceId);
            LoadSpace(space);
        }

        public void Show(GameObject anchor)
        {
            _currentAnchor = anchor;

            if (ChooserPanel != null)
                ChooserPanel.SetActive(true);

            LoadSpace(null);
            SelectButton.interactable = false;
        }

        private void LoadSpace(Space space)
        {
            _currentSpace = space;

            if (space != null && space.type == DataManager.RoomType)
            {
                ClearList();

                if (space.devices != null && space.devices.Any())
                {
                    foreach (var child in space.devices.OrderBy(d => d.name))
                    {
                        var obj = GameObject.Instantiate(DeviceItemPrefab);
                        var binding = obj.GetComponent<DeviceChooserBinding>();
                        binding.Context = child;

                        obj.transform.SetParent(ContentArea.transform, false);
                    }
                }
            }
            else
            {
                ClearList();

                IEnumerable<Space> childSpaces;

                if (space == null)
                {
                    BreadcrumbLabel.text = "Brands";
                    childSpaces = _dataManager.GetBrands();
                }
                else
                {
                    BreadcrumbLabel.text = space.name;
                    childSpaces = space.childSpaces;
                }

                if (childSpaces != null && childSpaces.Any())
                {

                    foreach (var child in childSpaces.OrderBy(s => s.name))
                    {
                        var obj = GameObject.Instantiate(SpaceItemPrefab);
                        var binding = obj.GetComponent<SpaceChooserBinding>();
                        binding.Context = child;

                        obj.transform.SetParent(ContentArea.transform, false);
                    }
                }
            }

            BackButton.SetActive(space != null);
        }

        private void ClearList()
        {
            while (ContentArea.transform.childCount > 0)
            {
                Transform child = ContentArea.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        public void OnSpaceSelected(Space space)
        {
            Debug.Log("DigitalTwinsChooser::OnSpaceChooserItemSelected");
            LoadSpace(_dataManager.GetSpaceById(space.id));
        }

        public void OnDeviceSelected(Device device)
        {
            Debug.Log("DigitalTwinsChooser::OnDeviceChooserItemSelected");
            _selectedDevice = _dataManager.GetDeviceBySpaceAndId(device.spaceId, device.id);
            SelectButton.interactable = true;
        }

        public bool IsSelected(Device device)
        {
            return _selectedDevice == device;
        }
    }

    public struct ChooserResult
    {
        public bool Cancelled;
        public Device SelectedDevice;
        public GameObject Anchor;
    }
}