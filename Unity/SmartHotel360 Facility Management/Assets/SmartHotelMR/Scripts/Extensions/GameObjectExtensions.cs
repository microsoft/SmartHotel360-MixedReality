using UnityEngine;

namespace SmartHotelMR
{
    public static class GameObjectExtensions
    {

        /// <summary>
        /// Gets the GameObject's root Parent object.
        /// </summary>
        /// <param name="child">The GameObject we're trying to find the root parent for.</param>
        /// <returns>The Root parent GameObject.</returns>
        public static GameObject GetParentRoot(this GameObject child)
        {
            if (child.transform.parent == null)
            {
                return child;
            }
            else
            {
                return GetParentRoot(child.transform.parent.gameObject);
            }
        }
    }
}
