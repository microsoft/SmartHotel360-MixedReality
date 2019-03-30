using UnityEngine;

namespace SmartHotelMR
{
    public class RotateAnimation : MonoBehaviour
    {
        [SerializeField]
        public float Speed = 100f;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up, Time.deltaTime * Speed);
        }
    }
}
