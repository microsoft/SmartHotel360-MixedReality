using UnityEngine;

namespace SmartHotelMR
{
    public class SpaceBinding : MonoBehaviour, ISpaceContext
    {
        [SerializeField]
        [Tooltip("GameObject that will display the brand model")]
        public GameObject ModelObject;

        [SerializeField]
        [Tooltip("TextMesh that will display the brand name")]
        public TextMesh LabelObject;

        private SmartHotelMR.Space _context;
        public SmartHotelMR.Space Context
        {
            get { return _context; }
            set
            {
                _context = value;
                UpdateBinding();
            }
        }

        // Use this for initialization
        void Start()
        {
            UpdateBinding();
        }

        private void UpdateBinding()
        {
            if (_context != null)
            {
                if (LabelObject != null)
                {
                    LabelObject.text = _context.name;
                }

                if (_context.type == DataManager.HotelBrandType)
                {
                    var dataManager = gameObject.GetComponentInParent<DataManager>();
                    if (dataManager != null)
                    {
                        var texture = dataManager.GetBrandImage(_context.id);

                        if (texture != null)
                        {
                            var renderer = ModelObject.GetComponent<Renderer>();
                            renderer.material.mainTexture = texture;
                        }
                    }
                }
            }
        }
    }
}