using UnityEngine;

namespace SmartHotelMR
{
    /// <summary>
    /// A Tagalong that stays at a fixed distance from the camera and always
    /// seeks to have a part of itself in the view frustum of the camera.
    /// </summary>
    [RequireComponent(typeof(Interpolator))]
    public class FixedTagalong : MonoBehaviour
    {
        // Simple Tagalongs seek to stay at a fixed distance from the Camera.
        [Tooltip("The distance in meters from the camera for the Tagalong to seek when updating its position.")]
        public float TagalongDistance = 2.0f;
        [Tooltip("If true, forces the Tagalong to be TagalongDistance from the camera, even if it didn't need to move otherwise.")]
        public bool EnforceDistance = true;

        [Tooltip("The speed at which to move the Tagalong when updating its position (meters/second).")]
        public float PositionUpdateSpeed = 9.8f;
        [Tooltip("When true, the Tagalong's motion is smoothed.")]
        public bool SmoothMotion = true;
        [Range(0.0f, 1.0f), Tooltip("The factor applied to the smoothing algorithm. 1.0f is super smooth. But slows things down a lot.")]
        public float SmoothingFactor = 0.75f;

        // The Interpolator is a helper class that handles various changes to an
        // object's transform. It is used by Tagalong to adjust the object's
        // transform.position.
        protected Interpolator interpolator;

        protected virtual void Start()
        {
            // Get the Interpolator component and set some default parameters for
            // it. These parameters can be adjusted in Unity's Inspector as well.
            interpolator = gameObject.GetComponent<Interpolator>();
            interpolator.SmoothLerpToTarget = SmoothMotion;
            interpolator.SmoothPositionLerpRatio = SmoothingFactor;
        }

        protected virtual void Update()
        {
            var toPosition = Camera.main.transform.position + Camera.main.transform.forward * TagalongDistance;
            interpolator.PositionPerSecond = PositionUpdateSpeed;
            interpolator.SetTargetPosition(toPosition);

            var rotationVector = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
            var toRotation = Quaternion.LookRotation(rotationVector, Vector3.up);
            interpolator.SetTargetLocalRotation(toRotation);
        }
    }
}