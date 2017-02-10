using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
namespace Detection
{
public class AILineOfSightDetection : MonoBehaviour
{
    [SerializeField]
    private GameObject Target; // used just for testing calculations.

    [Tooltip("field of view of the AI, this is used for things like line of sight.")]
    [SerializeField]
    float FOV;
    private float m_FOVHalfed; // This just stores half the fov as a pre-calculated float since it won't change.

    [Tooltip("Max range determines how far the far plane is defined.")]
    [SerializeField]
    private float MaxRange;

    [Tooltip("Min range determines how close the near plane is defined.")]
    [SerializeField]
    private float MinRange;

    [Tooltip("transform that will be used as apex/origin of camera frustum.")]
    [SerializeField]
    private Transform Apex;

    [Tooltip("Aspect defines the relationship between width and height of the frustum.")]
    [SerializeField]
    private float Aspect = 16.0f / 9.0f;

    [Tooltip("Layer mask for raycast to see if target is visible in line of sight.")]
    [SerializeField]
    private LayerMask SightRaycastLayerMask;

    [Tooltip("Maximum distance of raycast to attempt to find target in view.")]
    [SerializeField]
    private float RaycastMaxDistance;

    [Tooltip("Radius of cone used when firing numerous raycasts.")]
    [SerializeField]
    [Range(.001f, 8f)]
    private float ConeRadius;

    [Tooltip("Number of raycasts used in cone raycasting, this will also determine the spacing between the raycasts in the cone.")]
    [SerializeField]
    [Range(1, 8)]
    private uint NumberOfRaycasts = 1;

    [Tooltip("Number of rings in the raycast cone. (Number of raycasts * number of rings) is how many total raycasts there will be.")]
    [SerializeField]
    [Range(1, 2)]
    private uint NumberOfRings = 1;

    [Tooltip("Length of frustum forward debug line.")]
    [SerializeField]
    private float ForwardDebugLineLength;

    [Tooltip("Color of frustum drawn, why not? :)")]
    [SerializeField]
    private Color FrustumColor;

    [Tooltip("Color of frustum forward debug line, why not? :)")]
    [SerializeField]
    private Color ForwardDebugLineColor;

    [Tooltip("Color of plane normal debug lines, why not? :)")]
    [SerializeField]
    private Color PlaneNormalColor;

    [Tooltip("Color of raycast toward target when in view of AI.")]
    [SerializeField]
    private Color RaycastNotSeenColor;

    [Tooltip("Color of raycast toward target when seen by AI.")]
    [SerializeField]
    private Color RaycastSeenColor;

    [Tooltip("Color of raycast toward last seen target position.")]
    [SerializeField]
    private Color RaycastLastSeenColor;

    [Tooltip("Should we draw forward of frustum.")]
    [SerializeField]
    private bool IsDrawForward;

    [Tooltip("Should we draw raycast to target.")]
    [SerializeField]
    private bool IsDrawRaycastToTarget;

    [Tooltip("Should we draw frustum.")]
    [SerializeField]
    private bool IsDrawFrustum;

    [Tooltip("Draws the cone of raycasts if there is one.")]
    [SerializeField]
    private bool IsDrawConeRaycast;

    [Tooltip("Draws to last seen position of target from AI.")]
    [SerializeField]
    private bool IsDrawLastSeenPosition;

    private bool m_IsCanSeeTarget;
    public bool IsCanSeeTarget
    {
        get { return m_IsCanSeeTarget; }
    }

    private Vector3 m_TargetLastSeenPosition;
    public Vector3 TargetLastSeenPosition
    {
        get
        {
            return m_TargetLastSeenPosition;
        }
    }

    void Awake()
    {
        if (Target == null)
        {
            Debug.LogError("Target is null and must be set.");
        }
    }

    void Start()
    {
        m_FOVHalfed = FOV / 2;
    }

    void Update()
    {
        if (IsInPeripherals())
        {
            if (IsInLineOfSight())
            {
                m_IsCanSeeTarget = true;
            }
            else
            {
                m_IsCanSeeTarget = false;
            }
        }
        else
        {
            m_IsCanSeeTarget = false;
        }
    }

    // Shoot raycast at player to see if AI can see them.
    private bool IsInLineOfSight()
    {
        #region single raycast first
        // Shoot single raycast to see if we can see the target.
        RaycastHit raycastHit;
        Vector3 singleRaycastDir = (Target.transform.position - Apex.position).normalized;
        if (Physics.Raycast(Apex.position, singleRaycastDir, out raycastHit, RaycastMaxDistance, SightRaycastLayerMask))
        {
            Collider collider = raycastHit.collider;
            if (collider != null && collider.tag == "Player")
            {
                // Draw raycast with not seen color to indicate target seen by AI.
                if (IsDrawConeRaycast)
                {
                    Debug.DrawRay(Apex.position, RaycastMaxDistance * singleRaycastDir, RaycastSeenColor);
                }
                // Update target last seen position.
                m_TargetLastSeenPosition = Target.transform.position;
                return true;
            }
        }

        // Draw raycast with not seen color to indicate nothing being seen by AI with single raycast.
        if (IsDrawConeRaycast)
        {
            Debug.DrawRay(Apex.position, RaycastMaxDistance * singleRaycastDir, RaycastNotSeenColor);
        }
        #endregion

        // Shoot multiple raycasts more than 1 raycast was specified to shoot, in order ti see if we can see the target. 
        if (NumberOfRaycasts <= 1)
        {
            return false;
        }

        #region raycast in cone
        // Raycast in cone.
        float angleSpacing = 360.0f / NumberOfRaycasts;
        bool aRaycastHitTarget = false;
        for (int j = 0; j < NumberOfRings; j++)
        {
            float adjustedConeRadius = ConeRadius * (j + 1);
            for (int i = 0; i < NumberOfRaycasts; i++)
            {
                float currAngle = (i * angleSpacing) * Mathf.PI / 180.0f;
                Vector3 rightOfMiddleRaycast = Vector3.Cross(Apex.up, singleRaycastDir); // Need right vector of previous single raycast shot.
                Vector3 xOfTarget = Mathf.Cos(currAngle) * adjustedConeRadius * rightOfMiddleRaycast;
                Vector3 yOfTarget = Mathf.Sin(currAngle) * adjustedConeRadius * Apex.up;
                Vector3 zOfTarget = RaycastMaxDistance * singleRaycastDir; // Mathf.Sqrt(Mathf.Pow(ConeRadius, 2) + Mathf.Pow(RaycastMaxDistance, 2));
                Vector3 targetPos = xOfTarget + yOfTarget + zOfTarget + Apex.position;
                // Shoot single raycast to see if we can see the target.
                Vector3 dirVect = (targetPos - Apex.position).normalized;
                // Do raycasts.
                if (Physics.Raycast(Apex.position, dirVect, out raycastHit, RaycastMaxDistance, SightRaycastLayerMask))
                {
                    Collider collider = raycastHit.collider;
                    if (collider != null && collider.tag == "Player")
                    {
                        // Update target last seen position.
                        m_TargetLastSeenPosition = Target.transform.position;
                        // THIS BLOCK OF CODE SHOULD JUST RETURN TRUE BUT IF WE WANT TO DRAW RAY CONE WE CAN'T HENCE THE CHECK!
                        if (IsDrawConeRaycast)
                        {
                            Debug.DrawRay(Apex.position, targetPos - Apex.position, RaycastSeenColor);
                            aRaycastHitTarget = true;
                            continue;
                        }
                        else
                        {
                            // If we don't want to draw the raycast cone we can 
                            // return right away to allow for faster runtime.
                            return true;
                        }
                    }
                }

                // Draw failed raycast line for debug purposes.
                if (IsDrawConeRaycast)
                {
                    Debug.DrawRay(Apex.position, targetPos - Apex.position, RaycastNotSeenColor);
                }
            }
        }

        if (IsDrawConeRaycast)
        {
            return aRaycastHitTarget;
        }
        #endregion

        // If we got this far AI hasn't seen target.
        return false;
    }

    // Check to see if within peripherals.
    private bool IsInPeripherals()
    {
        Vector3 nTarget = (Target.transform.position - Apex.position).normalized;
        Vector3 nForward = Apex.forward;

        float dotResult = Vector3.Dot(nTarget, nForward);

        // Check to see if the target is directly in front or directly behind the AI.
        Vector3 dotCombinedWithForward = dotResult * nForward;
        if (dotCombinedWithForward == nForward) // this is done because cosine(90) is 0 so we need this edge case check.
        {
            return true;
        }
        else if (-dotCombinedWithForward == -nForward) // this is done because cosine(180) is 0 so we need this edge case check.
        {
            return false;
        }

        // If passed edge cases then see if target is not within peripherals.
        float angle = Mathf.Acos(dotResult) * 180.0f / Mathf.PI;
        if (angle > m_FOVHalfed)
        {
            return false;
        }

        // Passed all checks so it is within peripherals.
        return true;
    }

    void OnDrawGizmos()
    {
        // Set position of gizmo to be where the frustum Apex is defined.
        Gizmos.matrix = Matrix4x4.TRS(Apex.position, Apex.rotation, Vector3.one);
        Gizmos.color = FrustumColor;
        if (IsDrawFrustum)
        {
            Gizmos.DrawFrustum(Vector3.zero, FOV, MaxRange, MinRange, Aspect);
        }

        // Draw forward of frustum.
        Gizmos.color = ForwardDebugLineColor;
        if (IsDrawForward)
        {
            Debug.DrawRay(Apex.position, Apex.forward * ForwardDebugLineLength, ForwardDebugLineColor);
        }

        if (IsDrawLastSeenPosition && !m_IsCanSeeTarget)
        {
            Debug.DrawRay(Apex.position, m_TargetLastSeenPosition - Apex.position, RaycastLastSeenColor);
        }
    }
}
}
}