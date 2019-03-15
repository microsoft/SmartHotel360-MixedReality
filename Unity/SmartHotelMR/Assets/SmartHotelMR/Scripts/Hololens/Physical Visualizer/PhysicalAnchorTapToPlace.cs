using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using SmartHotelMR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Enumeration containing the surfaces on which a GameObject
/// can be placed.  For simplicity of this sample, only one
/// surface type is allowed to be selected.
/// </summary>
public enum PlacementSurfaces
{
    // Horizontal surface with an upward pointing normal.    
    Horizontal = 1,

    // Vertical surface with a normal facing the user.
    Vertical = 2,
}

public class PhysicalAnchorTapToPlace : MonoBehaviour, IInputClickHandler
{
    //private PlacementSurfaces PlacementSurface = PlacementSurfaces.Horizontal | PlacementSurfaces.Vertical;

    // The most recent distance to the surface.This is used to
    // locate the object when the user's gaze does not intersect
    // with the Spatial Mapping mesh.
    private float lastDistance = 2.0f;

    // The distance away from the target surface that the object should hover prior while being placed.
    private float hoverDistance = 0.01f; //0.15f

    // Threshold (the closer to 0, the stricter the standard) used to determine if a surface is flat.
    //private float distanceThreshold = 0.02f;

    // Threshold (the closer to 1, the stricter the standard) used to determine if a surface is vertical.
    private float upNormalThreshold = 0.7f; //was 0.9

    // Maximum distance, from the object, that placement is allowed.
    // This is used when raycasting to see if the object is near a placeable surface.
    private float maximumPlacementDistance = 5.0f;

    // Speed (1.0 being fastest) at which the object settles to the surface upon placement.
    private float placementVelocity = 0.33f; //was 0.06f

    // The location at which the object will be placed.
    private Vector3 targetPosition;

    [Tooltip("Cursor used to indicate valid/invalid surfaces and show placement location")]
    public GameObject[] PlacementCursors;

    [Tooltip("Material to assign to ZeroPointCursor when on an invalid surface")]
    public Material InvalidMaterial;

    [Tooltip("Material to assign to ZeroPointCursor when on a valid surface")]
    public Material ValidMaterial;

    public BoxCollider BoxCollider;

    public int ColliderLayer
    {
        get { return 1 << UnityEngine.LayerMask.NameToLayer("Spatial"); }
    }

    void OnEnable()
    {
        InputManager.Instance.PushFallbackInputHandler(gameObject);
        SpatialMappingManager.Instance.StartObserver();
        SetupCollider();
    }

    void OnDisable()
    {
        InputManager.Instance.PopFallbackInputHandler();
        SpatialMappingManager.Instance.CleanupObserver();
    }

    private void SetupCollider()
    {
        targetPosition = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.activeSelf)
        {
            Move();
            ValidatePlacement();
        }
    }

    private bool ValidatePlacement()
    {
        bool isVerticalValid = ValidateVerticalPlacement();
        bool isHorizontalValid = ValidateHorizontalPlacement();

        if (isHorizontalValid || isVerticalValid)
        {
            ApplyValidMaterial();
            return true;
        }
        else
        {
            ApplyInvalidMaterial();
            return false;
        }
    }

    private bool ValidateVerticalPlacement()
    {
        Vector3 position;
        Vector3 surfaceNormal;

        Vector3 raycastDirection = gameObject.transform.forward;

        // Initialize out parameters.
        position = Vector3.zero;
        surfaceNormal = Vector3.zero;

        Vector3[] facePoints = GetColliderFacePoints(false);

        // The origin points we receive are in local space and we 
        // need to raycast in world space.
        for (int i = 0; i < facePoints.Length; i++)
        {
            facePoints[i] = gameObject.transform.TransformVector(facePoints[i]) + gameObject.transform.position;
        }

        // Cast a ray from the center of the box collider face to the surface.
        RaycastHit centerHit;
        if (!Physics.Raycast(facePoints[0],
                        raycastDirection,
                        out centerHit,
                        maximumPlacementDistance,
                        ColliderLayer))
        {
            // If the ray failed to hit the surface, we are done.
            return false;
        }

        return true;

        // We have found a surface.  Set position and surfaceNormal.
        position = centerHit.point;
        surfaceNormal = centerHit.normal;

        // Cast a ray from the corners of the box collider face to the surface.
        for (int i = 1; i < facePoints.Length; i++)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(facePoints[i],
                                raycastDirection,
                                out hitInfo,
                                maximumPlacementDistance,
                                ColliderLayer))
            {
                //// To be a valid placement location, each of the corners must have a similar
                //// enough distance to the surface as the center point
                //if (!IsEquivalentDistance(centerHit.distance, hitInfo.distance))
                //{
                //    return false;
                //}
            }
            else
            {
                // The raycast failed to intersect with the target layer.
                return false;
            }
        }

        return true;
    }

    private bool ValidateHorizontalPlacement()
    {
        Vector3 position;
        Vector3 surfaceNormal;

        Vector3 raycastDirection = gameObject.transform.forward;
        raycastDirection = -(Vector3.up);

        // Initialize out parameters.
        position = Vector3.zero;
        surfaceNormal = Vector3.zero;

        Vector3[] facePoints = GetColliderFacePoints();

        // The origin points we receive are in local space and we 
        // need to raycast in world space.
        for (int i = 0; i < facePoints.Length; i++)
        {
            facePoints[i] = gameObject.transform.TransformVector(facePoints[i]) + gameObject.transform.position;
        }

        // Cast a ray from the center of the box collider face to the surface.
        RaycastHit centerHit;
        if (!Physics.Raycast(facePoints[0],
                        raycastDirection,
                        out centerHit,
                        maximumPlacementDistance,
                        ColliderLayer))
        {
            // If the ray failed to hit the surface, we are done.
            return false;
        }

        return true;

        // We have found a surface.  Set position and surfaceNormal.
        position = centerHit.point;
        surfaceNormal = centerHit.normal;

        // Cast a ray from the corners of the box collider face to the surface.
        for (int i = 1; i < facePoints.Length; i++)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(facePoints[i],
                                raycastDirection,
                                out hitInfo,
                                maximumPlacementDistance,
                                ColliderLayer))
            {
                //// To be a valid placement location, each of the corners must have a similar
                //// enough distance to the surface as the center point
                //if (!IsEquivalentDistance(centerHit.distance, hitInfo.distance))
                //{
                //    return false;
                //}
            }
            else
            {
                // The raycast failed to intersect with the target layer.
                return false;
            }
        }

        return true;
    }

    private void ApplyValidMaterial()
    {
        if (ValidMaterial)
        {
            foreach (var cursor in PlacementCursors)
            {
                var mr = cursor.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material = ValidMaterial;
                }
            }
        }
    }

    private void ApplyInvalidMaterial()
    {
        if (InvalidMaterial)
        {
            foreach (var cursor in PlacementCursors)
            {
                var mr = cursor.GetComponent<MeshRenderer>();
                if (mr)
                {
                    mr.material = InvalidMaterial;
                }
            }
        }
    }

    /// <summary>
    /// Determine the coordinates, in local space, of the box collider face that 
    /// will be placed against the target surface.
    /// </summary>
    /// <returns>
    /// Vector3 array with the center point of the face at index 0.
    /// </returns>
    private Vector3[] GetColliderFacePoints(bool horizontal = true)
    {
        // Get the collider extents.  
        // The size values are twice the extents.
        Vector3 extents = BoxCollider.size / 2;

        // Calculate the min and max values for each coordinate.
        float minX = BoxCollider.center.x - extents.x;
        float maxX = BoxCollider.center.x + extents.x;
        float minY = BoxCollider.center.y - extents.y;
        float maxY = BoxCollider.center.y + extents.y;
        float minZ = BoxCollider.center.z - extents.z;
        float maxZ = BoxCollider.center.z + extents.z;

        Vector3 center;
        Vector3 corner0;
        Vector3 corner1;
        Vector3 corner2;
        Vector3 corner3;

        if (horizontal)
        {
            // Placing on horizontal surfaces.
            center = new Vector3(BoxCollider.center.x, minY, BoxCollider.center.z);
            corner0 = new Vector3(minX, minY, minZ);
            corner1 = new Vector3(minX, minY, maxZ);
            corner2 = new Vector3(maxX, minY, minZ);
            corner3 = new Vector3(maxX, minY, maxZ);
        }
        else
        {
            // Placing on vertical surfaces.
            center = new Vector3(BoxCollider.center.x, BoxCollider.center.y, maxZ);
            corner0 = new Vector3(minX, minY, maxZ);
            corner1 = new Vector3(minX, maxY, maxZ);
            corner2 = new Vector3(maxX, minY, maxZ);
            corner3 = new Vector3(maxX, maxY, maxZ);
        }

        return new Vector3[] { center, corner0, corner1, corner2, corner3 };
    }

    /// <summary>
    /// Positions the object along the surface toward which the user is gazing.
    /// </summary>
    /// <remarks>
    /// If the user's gaze does not intersect with a surface, the object
    /// will remain at the most recently calculated distance.
    /// </remarks>
    private void Move()
    {
        Vector3 moveTo = gameObject.transform.position;
        Vector3 surfaceNormal = Vector3.zero;
        RaycastHit hitInfo;

        bool hit = Physics.Raycast(Camera.main.transform.position,
                                Camera.main.transform.forward,
                                out hitInfo,
                                20f,
                                ColliderLayer);

        if (hit)
        {
            float offsetDistance = hoverDistance;

            // Place the object a small distance away from the surface while keeping 
            // the object from going behind the user.
            if (hitInfo.distance <= hoverDistance)
            {
                offsetDistance = 0f;
            }

            moveTo = hitInfo.point + (offsetDistance * hitInfo.normal);

            lastDistance = hitInfo.distance;
            surfaceNormal = hitInfo.normal;
        }
        else
        {
            // The raycast failed to hit a surface.  In this case, keep the object at the distance of the last
            // intersected surface.
            moveTo = Camera.main.transform.position + (Camera.main.transform.forward * lastDistance);
        }

        // Follow the user's gaze.
        float dist = Mathf.Abs((gameObject.transform.position - moveTo).magnitude);
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, moveTo, placementVelocity / dist);

        // Orient the object.
        // We are using the return value from Physics.Raycast to instruct
        // the OrientObject function to align to the vertical surface if appropriate.
        OrientObject(hit, surfaceNormal);
    }

    /// <summary>
    /// Orients the object so that it faces the user.
    /// </summary>
    /// <param name="alignToVerticalSurface">
    /// If true and the object is to be placed on a vertical surface, 
    /// orient parallel to the target surface.  If false, orient the object 
    /// to face the user.
    /// </param>
    /// <param name="surfaceNormal">
    /// The target surface's normal vector.
    /// </param>
    /// <remarks>
    /// The aligntoVerticalSurface parameter is ignored if the object
    /// is to be placed on a horizontalSurface
    /// </remarks>
    private void OrientObject(bool isOnSurface, Vector3 surfaceNormal)
    {
        Quaternion rotation = Camera.main.transform.localRotation;

        if (isOnSurface)
        {
            if (Mathf.Abs(surfaceNormal.y) <= (1 - upNormalThreshold))
            {
                rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
            }
            else
            {
                rotation.x = 0f;
                rotation.z = 0f;
            }
        }
        else
        {
            rotation.x = 0f;
            rotation.z = 0f;
        }

        gameObject.transform.rotation = rotation;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (ValidatePlacement())
        {
            ExecuteEvents.ExecuteHierarchy<IAnchorMessageTarget>(
                gameObject,
                null,
                (x, y) => x.OnSetPhysicalVisualizerAnchor(new SpawnData()
                {
                    Position = gameObject.transform.position,
                    Rotation = gameObject.transform.rotation
                }));
        }
    }
}
