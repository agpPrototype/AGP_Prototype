using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    namespace Detection
    {
        public class AILineOfSightDetection : MonoBehaviour
        {
            /* Field of view regions that AI have different difficulties seeing.
             This can be set by designers.*/
            public enum FOV_REGION
            {
                DIRECT,         
                SIDE,           
                PERIPHERAL,
                NO_REGION
            }

            #region Member Variables
            [SerializeField]
            private GameObject Target; // used just for testing calculations.

            [Tooltip("field of view of the AI, this is used for things like line of sight.")]
            [SerializeField, HideInInspector]
            private float FOV;
            private float m_FOVHalfed; // This just stores half the fov as a pre-calculated float since it won't change.
            
            /// <summary>
            /// Direct field of view variables.
            /// </summary>
            [Tooltip("Direct field of view of the AI. Anything in this area is most likely to be seen.")]
            [SerializeField, HideInInspector]
            private float DirectFOVPercentage;
            private float m_DirectFOVHalfed;
            [Tooltip("Color of direct field of view")]
            [SerializeField, HideInInspector]
            private Color DirectFOVColor;
            [Tooltip("Maximum distance of raycast to attempt to find target in view.")]
            [SerializeField, HideInInspector]
            private float DirectRaycastMaxDistance;
            [Tooltip("Radius of cone used when firing numerous raycasts.")]
            [SerializeField, HideInInspector]
            private float DirectConeRadius;
            [Tooltip("Number of raycasts used in cone raycasting, this will also determine the spacing between the raycasts in the cone.")]
            [SerializeField, HideInInspector]
            private int DirectNumbOfRaycasts = 1;
            [Tooltip("Number of rings in the raycast cone. (Number of raycasts * number of rings) is how many total raycasts there will be.")]
            [SerializeField, HideInInspector]
            private int DirectNumbOfRings = 1;


            /// <summary>
            /// Side field of view variables.
            /// </summary>
            [Tooltip("Side field of view of the AI. Anything in this area is decently likely to be seen.")]
            [SerializeField, HideInInspector]
            private float SideFOVPercentage;
            private float m_SideFOVHalfed;
            [Tooltip("Color of side field of view")]
            [SerializeField, HideInInspector]
            private Color SideFOVColor;
            [Tooltip("Maximum distance of raycast to attempt to find target in view.")]
            [SerializeField, HideInInspector]
            private float SideRaycastMaxDistance;
            [Tooltip("Radius of cone used when firing numerous raycasts.")]
            [SerializeField, HideInInspector]
            private float SideConeRadius;
            [Tooltip("Number of raycasts used in cone raycasting, this will also determine the spacing between the raycasts in the cone.")]
            [SerializeField, HideInInspector]
            private int SideNumbOfRaycasts = 1;
            [Tooltip("Number of rings in the raycast cone. (Number of raycasts * number of rings) is how many total raycasts there will be.")]
            [SerializeField, HideInInspector]
            private int SideNumbOfRings = 1;

            /// <summary>
            /// Peripheral field of view variables.
            /// </summary>
            [Tooltip("Peripheral field of view of the AI. Anything in this area is unlikely to be seen.")]
            [SerializeField, HideInInspector]
            private float PeriphFOVPercentage;
            private float m_PeriphFOVHalfed;
            [Tooltip("Color of peripheral field of view")]
            [SerializeField, HideInInspector]
            private Color PeriphFOVColor;
            [Tooltip("Maximum distance of raycast to attempt to find target in view.")]
            [SerializeField, HideInInspector]
            private float PeriphRaycastMaxDistance;
            [Tooltip("Radius of cone used when firing numerous raycasts.")]
            [SerializeField, HideInInspector]
            private float PeriphConeRadius;
            [Tooltip("Number of raycasts used in cone raycasting, this will also determine the spacing between the raycasts in the cone.")]
            [SerializeField, HideInInspector]
            private int PeriphNumbOfRaycasts = 1;
            [Tooltip("Number of rings in the raycast cone. (Number of raycasts * number of rings) is how many total raycasts there will be.")]
            [SerializeField, HideInInspector]
            private int PeriphNumbOfRings = 1;

            [Tooltip("transform that will be used as apex/origin of camera frustum.")]
            [SerializeField]
            private Transform Apex;

            [Tooltip("Layer mask for raycast to see if target is visible in line of sight.")]
            [SerializeField]
            private LayerMask SightRaycastLayerMask;

            [Tooltip("Length of frustum forward debug line.")]
            [SerializeField]
            private float ForwardDebugLineLength;

            [Tooltip("Color of frustum forward debug line, why not? :)")]
            [SerializeField]
            private Color ForwardDebugLineColor;

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
            #endregion
            
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
                m_DirectFOVHalfed = DirectFOVPercentage * FOV / 2;
                m_SideFOVHalfed = SideFOVPercentage * FOV / 2;
                m_PeriphFOVHalfed = PeriphFOVPercentage * FOV / 2;
            }

            void Update()
            {
                if (IsInLineOfSight(Target))
                {
                    m_IsCanSeeTarget = true;
                }
                else
                {
                    m_IsCanSeeTarget = false;
                }
            }

            /* This will get the angle from the forward vector of the AI to the
             target fed into this function. */
            private float getAngleToTarget(GameObject target)
            {
                Vector3 nTarget = (target.transform.position - Apex.position).normalized;
                Vector3 nForward = Apex.forward;
                float dotResult = Vector3.Dot(nTarget, nForward);
                return Mathf.Acos(dotResult) * 180.0f / Mathf.PI;
            }

            /* Gets which FOV_REGION the target is in */
            private FOV_REGION getFOVRegion(GameObject target)
            {
                float angleToTarget = getAngleToTarget(target);
                if (angleToTarget <= m_DirectFOVHalfed)
                {
                    return FOV_REGION.DIRECT;
                }
                else if(angleToTarget <= m_DirectFOVHalfed + m_SideFOVHalfed)
                {
                    return FOV_REGION.SIDE;
                }
                else if (angleToTarget <= m_DirectFOVHalfed + m_SideFOVHalfed + m_PeriphFOVHalfed)
                {
                    return FOV_REGION.PERIPHERAL;
                }
                else
                {
                    return FOV_REGION.NO_REGION;
                }
            }

            // Shoot raycast at player to see if AI can see them.
            private bool IsInLineOfSight(GameObject target)
            {
                FOV_REGION fovRegion = getFOVRegion(target);
                bool isHitTarget = false;
                switch (fovRegion)
                {
                    case FOV_REGION.NO_REGION:
                        return false;
                        break;

                    case FOV_REGION.DIRECT:
                        isHitTarget = raycastOneRayTowardTarget(DirectRaycastMaxDistance);
                        if(!isHitTarget)
                        {
                            isHitTarget = raycastInConeTowardTarget(DirectNumbOfRaycasts, DirectNumbOfRings, DirectConeRadius, DirectRaycastMaxDistance);
                        }
                        return isHitTarget;
                        break;

                    case FOV_REGION.SIDE:
                        isHitTarget = raycastOneRayTowardTarget(SideRaycastMaxDistance);
                        if (!isHitTarget)
                        {
                            isHitTarget = raycastInConeTowardTarget(SideNumbOfRaycasts, SideNumbOfRings, SideConeRadius, SideRaycastMaxDistance);
                        }
                        return isHitTarget;
                        break;

                    case FOV_REGION.PERIPHERAL:
                        isHitTarget = raycastOneRayTowardTarget(PeriphRaycastMaxDistance);
                        return isHitTarget;
                        break;
                }
                return isHitTarget;
            }

            private bool raycastInConeTowardTarget(int numRaycasts, int numRings, float coneRadius, float raycastMaxDistance)
            {
                // If we were fed 
                if(numRaycasts <= 1)
                {
                    Debug.LogWarning("Tried to raycast in a cone with just one raycast.");
                    return false;
                }

                // Raycast in cone.
                float angleSpacing = 360.0f / numRaycasts;
                Vector3 targetDir = (Target.transform.position - Apex.position).normalized;
                bool aRaycastHitTarget = false;
                RaycastHit raycastHit;
                for (int j = 0; j < numRings; j++)
                {
                    float adjustedConeRadius = coneRadius * (j + 1);
                    for (int i = 0; i < numRaycasts; i++)
                    {
                        float currAngle = (i * angleSpacing) * Mathf.PI / 180.0f;
                        Vector3 rightOfMiddleRaycast = Vector3.Cross(Apex.up, targetDir); // Need right vector of previous single raycast shot.
                        Vector3 xOfTarget = Mathf.Cos(currAngle) * adjustedConeRadius * rightOfMiddleRaycast;
                        Vector3 yOfTarget = Mathf.Sin(currAngle) * adjustedConeRadius * Apex.up;
                        Vector3 zOfTarget = raycastMaxDistance * targetDir; // Mathf.Sqrt(Mathf.Pow(ConeRadius, 2) + Mathf.Pow(RaycastMaxDistance, 2));
                        Vector3 targetPos = xOfTarget + yOfTarget + zOfTarget + Apex.position;
                        // Shoot single raycast to see if we can see the target.
                        Vector3 dirVect = (targetPos - Apex.position).normalized;
                        // Do raycasts.
                        if (Physics.Raycast(Apex.position, dirVect, out raycastHit, raycastMaxDistance, SightRaycastLayerMask))
                        {
                            Collider collider = raycastHit.collider;
                            if (collider != null && collider.tag == "Player")
                            {
                                // Update target last seen position.
                                m_TargetLastSeenPosition = Target.transform.position;

                                /* If in editor mode then draw this ray and continue so that it draws 
                                 * all the others as well for debugging purposes. */
                                #if UNITY_EDITOR
                                if (IsDrawConeRaycast)
                                {
                                    Debug.DrawRay(Apex.position, targetPos - Apex.position, RaycastSeenColor);
                                    aRaycastHitTarget = true;
                                    continue;
                                }
                                #endif

                                // If we don't want to draw the raycast cone we can 
                                // return right away to allow for faster runtime.
                                return true;
                            }
                        }

                        #if UNITY_EDITOR
                        // Draw failed raycast line for debug purposes.
                        if (IsDrawConeRaycast)
                        {
                            Debug.DrawRay(Apex.position, targetPos - Apex.position, RaycastNotSeenColor);
                        }
                        #endif
                    }
                }

                #if UNITY_EDITOR
                if (IsDrawConeRaycast)
                {
                    return aRaycastHitTarget;
                }
                #endif

                return false; // If we got this far no raycast hit.
            }

            private bool raycastOneRayTowardTarget(float raycastMaxDistance)
            {
                // Shoot single raycast to see if we can see the target.
                RaycastHit raycastHit;
                Vector3 singleRaycastDir = (Target.transform.position - Apex.position).normalized;
                if (Physics.Raycast(Apex.position, singleRaycastDir, out raycastHit, raycastMaxDistance, SightRaycastLayerMask))
                {
                    Collider collider = raycastHit.collider;
                    if (collider != null && collider.tag == "Player")
                    {
                        #if UNITY_EDITOR
                        // Draw raycast with not seen color to indicate target seen by AI.
                        if (IsDrawConeRaycast)
                        {
                            Debug.DrawRay(Apex.position, raycastMaxDistance * singleRaycastDir, RaycastSeenColor);
                        }
                        #endif
                        // Update target last seen position.
                        m_TargetLastSeenPosition = Target.transform.position;
                        return true;
                    }
                }

                #if UNITY_EDITOR
                // Draw raycast with not seen color to indicate nothing being seen by AI with single raycast.
                if (IsDrawConeRaycast)
                {
                    Debug.DrawRay(Apex.position, raycastMaxDistance * singleRaycastDir, RaycastNotSeenColor);
                }
                #endif

                return false;
            }

            #if UNITY_EDITOR
            void OnDrawGizmos()
            {
                // Set position of gizmo to be where the frustum Apex is defined.
                /*Gizmos.matrix = Matrix4x4.TRS(Apex.position, Apex.rotation, Vector3.one);
                Gizmos.color = FrustumColor;
                if (IsDrawFrustum)
                {
                    Gizmos.DrawFrustum(Vector3.zero, FOV, MaxRange, MinRange, Aspect);
                }*/

                if(IsDrawFrustum)
                {
                    Gizmos.color = DirectFOVColor;
                    float angleOfLines = m_DirectFOVHalfed;
                    Vector3 vRightView = Quaternion.AngleAxis(angleOfLines, this.transform.up) * this.transform.forward * DirectRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vRightView);
                    Vector3 vLeftView = Quaternion.AngleAxis(-angleOfLines, this.transform.up) * this.transform.forward * DirectRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vLeftView);

                    Gizmos.color = SideFOVColor;
                    angleOfLines = m_DirectFOVHalfed + m_SideFOVHalfed;
                    vRightView = Quaternion.AngleAxis(angleOfLines, this.transform.up) * this.transform.forward * SideRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vRightView);
                    vLeftView = Quaternion.AngleAxis(-angleOfLines, this.transform.up) * this.transform.forward * SideRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vLeftView);

                    Gizmos.color = PeriphFOVColor;
                    angleOfLines = m_DirectFOVHalfed + m_SideFOVHalfed + m_PeriphFOVHalfed;
                    vRightView = Quaternion.AngleAxis(angleOfLines, this.transform.up) * this.transform.forward * PeriphRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vRightView);
                    vLeftView = Quaternion.AngleAxis(-angleOfLines, this.transform.up) * this.transform.forward * PeriphRaycastMaxDistance;
                    Gizmos.DrawLine(Apex.position, Apex.position + vLeftView);
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
            #endif
        }
    }
}