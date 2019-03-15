using UnityEngine;

namespace SmartHotelMR
{
    public class ProximityVisibility : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The object to show when user is within proximity")]
        public GameObject Object;

        [SerializeField]
        [Tooltip("The proximity within which the object should be visible")]
        public float ProximityDistance;

        void Update()
        {
            if (Object != null)
            {
#if WINDOWS_UWP
                // When using the Hololens, calculate distance purely on horizontal distance 
                // to not require use to move their face up/down towards the object
                var cameraPosition = Camera.main.transform.position;
                var objectPosition = new Vector3(gameObject.transform.position.x, Camera.main.transform.position.y, gameObject.transform.position.z);

                float distance = Vector3.Distance(cameraPosition, objectPosition);
#else
                float distance = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
#endif
                Object.SetActive(distance <= ProximityDistance);
            }
        }
    }
}
