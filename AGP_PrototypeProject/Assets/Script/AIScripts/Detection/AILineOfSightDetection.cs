using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// AUTHOR: rob neir
/// 
/// DESCRIPTION: When attached to game object gives it access to functions that
/// allow it to have a line of sight that picks up "AIVisibles".
/// 
/// </summary>
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
            private Transform m_Apex;
            public Transform Apex { get { return m_Apex; } }

			[Tooltip("gameobject that represents when the enemy is alerted to the player.")]
			[SerializeField]
			private EnemyDetectionBeam m_Beam;
			public EnemyDetectionBeam Beam { get { return m_Beam; } }

            [Tooltip("Layer mask for raycast to see if target is visible in line of sight.")]
            [SerializeField]
            private LayerMask SightRaycastLayerMask;

            [Tooltip("Color of raycast toward target when in view of AI.")]
            [SerializeField]
            private Color RaycastNotSeenColor;

            [Tooltip("Color of raycast toward target when seen by AI.")]
            [SerializeField]
            private Color RaycastSeenColor;

            [Tooltip("Should we draw frustum.")]
            [SerializeField]
            private bool IsDrawFrustum;

            [Tooltip("Draws the cone of raycasts if there is one.")]
            [SerializeField]
            private bool IsDrawConeRaycast;

            #endregion
            
            void Start()
            {
                m_FOVHalfed = FOV / 2;
                m_DirectFOVHalfed = DirectFOVPercentage * FOV / 2;
                m_SideFOVHalfed = SideFOVPercentage * FOV / 2;
                m_PeriphFOVHalfed = PeriphFOVPercentage * FOV / 2;
            }

            /* gets the max distance that AI can see within a specific FOV region. */
            public float getFOVViewDistance(FOV_REGION fovRegion)
            {
                if (fovRegion == FOV_REGION.DIRECT)
                    return DirectRaycastMaxDistance;
                else if (fovRegion == FOV_REGION.SIDE)
                    return SideRaycastMaxDistance;
                else if (fovRegion == FOV_REGION.PERIPHERAL)
                    return PeriphRaycastMaxDistance;
                else
                    return 0;
            }

            public Vector3 GetFOVVector(Vector3 forward, Vector3 up, FOV_REGION fovRegion, bool wantRightVector, bool wantNormalized)
            {
                // figure out what angle to rotate vector by depending on region requested.
                float angle = 0.0f;
                if (fovRegion == FOV_REGION.DIRECT)
                {
                    angle = m_DirectFOVHalfed;
                }
                else if (fovRegion == FOV_REGION.PERIPHERAL)
                {
                    angle = m_DirectFOVHalfed + m_SideFOVHalfed + m_PeriphFOVHalfed;
                }
                else if (fovRegion == FOV_REGION.SIDE)
                {
                    angle = m_DirectFOVHalfed + m_SideFOVHalfed;
                }

                // return either normalized or non-normalized vector representing FOV vector.
                Vector3 normalized;
                if (wantRightVector)
                {
                    normalized = (Quaternion.AngleAxis(angle, up) * forward).normalized;
                }
                else
                {
                    normalized = (Quaternion.AngleAxis(-angle, up) * forward).normalized;
                }
                return wantNormalized ? normalized : normalized * getFOVViewDistance(fovRegion);
            }

            /* gets highest priority visible based on AILineOfSightDetection settings */
            public AIVisible GetHighestThreat(GameObject detector)
            {
                List<AIVisible> actualVisibles = GetVisibles();
                AIVisible highestThreat = null;
                if (actualVisibles.Count > 0)
                {
                    for (int i = 0; i < actualVisibles.Count; i++)
                    {
                        AIVisible currVisible = actualVisibles[i];

                        // if visible component is not enabled then continue.
                        if (!currVisible.enabled)
                        {
                            continue;
                        }

                        if (highestThreat)
                        {
                            if (currVisible.ThreatLevel > highestThreat.ThreatLevel)
                            {
                                highestThreat = currVisible;
                            }
                            else if (currVisible.ThreatLevel == highestThreat.ThreatLevel) // deal with ties based on detector's distance from threat.
                            {
                                if (detector != null)
                                {
                                    // check to see if new visible target is closer.
                                    if ((detector.transform.position - currVisible.transform.position).sqrMagnitude <
                                        (detector.transform.position - highestThreat.transform.position).sqrMagnitude)
                                    {
                                        highestThreat = currVisible;
                                    }
                                }
                            }
                        }
                        else
                        {
                            highestThreat = currVisible;
                        }
                    }
                }

                return highestThreat;
            }

            /* gets a list of visibles that the AI can see. */
            public List<AIVisible> GetVisibles()
            {
                List<AIVisible> possibleVisibles = DetectionManager.Instance.Visibles;
                List<AIVisible> actualVisibles = new List<AIVisible>();
                for (int i = 0; i < possibleVisibles.Count; i++)
                {
                    AIVisible visible = possibleVisibles[i];
                    // Check to see if we can see the target.
                    if (IsInLineOfSight(visible))
                    {
                        actualVisibles.Add(visible);
                    }
                }
                return actualVisibles;
            }

            /* This will get the angle from the forward vector of the AI to the
             target fed into this function. */
            private float getAngleToTarget(AIVisible target)
            {
                // Take out any Y values of target location because we don't care about height of target for this calculation.
                Vector3 targetPosNoYComponent = new Vector3(target.transform.position.x, 0, target.transform.position.z);
                Vector3 apexPosNoYComponent = new Vector3(m_Apex.transform.position.x, 0, m_Apex.transform.position.z);
                Vector3 nTargetPosNoYComponent = (targetPosNoYComponent - apexPosNoYComponent).normalized;

                Vector3 nForward = m_Apex.forward;
                Vector3 forwardNoYComponent = new Vector3(nForward.x, 0, nForward.z).normalized;

                float dotResult = Vector3.Dot(nTargetPosNoYComponent, forwardNoYComponent);
                float angle = Mathf.Acos(dotResult) * 180.0f / Mathf.PI;

                return angle;
            }

            private bool isInFOV(FOV_REGION fovRegion, AIVisible target)
            {
                // get vector to target.
                Vector3 vToTarget = (target.transform.position - this.Apex.position);
                // need forward of AI vision.
                Vector3 apexForward = Apex.transform.forward;
                // dot the normalized dir to target with forward for calculations below.
                float targetDotForward = Vector3.Dot(vToTarget.normalized, apexForward);

                // get non normalized FOV vector.
                Vector3 vFOVVector = GetFOVVector(Apex.forward, Apex.up, fovRegion, true, true);
                // normalize vector to then use to see if target is within FOV angle.
                float fovDotForward = Vector3.Dot(vFOVVector.normalized, apexForward);
                if (targetDotForward > fovDotForward)
                {
                    // check to see if AI can see the target in terms of distance.
                    if (vFOVVector.sqrMagnitude <= vToTarget.sqrMagnitude)
                    {
                        return true;
                    }
                }

                return false;
            }

            /* Gets which FOV_REGION the target is in but does not take into account the distance
             * to which the AI can see in that field of view. This is purely based on angles. */
            private FOV_REGION getFOVRegion(AIVisible target)
            {
                if (isInFOV(FOV_REGION.PERIPHERAL, target))
                {
                    if (isInFOV(FOV_REGION.SIDE, target))
                    {
                        if (isInFOV(FOV_REGION.DIRECT, target))
                        {
                            return FOV_REGION.DIRECT;
                        }
                        return FOV_REGION.SIDE;
                    }
                    return FOV_REGION.PERIPHERAL;
                }
                return FOV_REGION.NO_REGION;
            }

            // Shoot raycast at player to see if AI can see them.
            private bool IsInLineOfSight(AIVisible target)
            {
                FOV_REGION fovRegion = getFOVRegion(target);
                bool isHitTarget = false;
                switch (fovRegion)
                {
                    case FOV_REGION.NO_REGION:
                        return false;
                        break;

                    case FOV_REGION.DIRECT:
                        isHitTarget = raycastOneRayTowardTarget(target, DirectRaycastMaxDistance);
                        if(!isHitTarget)
                        {
                            isHitTarget = raycastInConeTowardTarget(target, DirectNumbOfRaycasts, 
                                DirectNumbOfRings, DirectConeRadius, DirectRaycastMaxDistance);
                        }
                        return isHitTarget;
                        break;

                    case FOV_REGION.SIDE:
                        isHitTarget = raycastOneRayTowardTarget(target, SideRaycastMaxDistance);
                        if (!isHitTarget)
                        {
                            isHitTarget = raycastInConeTowardTarget(target, SideNumbOfRaycasts, 
                                SideNumbOfRings, SideConeRadius, SideRaycastMaxDistance);
                        }
                        return isHitTarget;
                        break;

                    case FOV_REGION.PERIPHERAL:
                        isHitTarget = raycastOneRayTowardTarget(target, PeriphRaycastMaxDistance);
                        if (!isHitTarget)
                        {
                            isHitTarget = raycastInConeTowardTarget(target, PeriphNumbOfRaycasts, 
                                PeriphNumbOfRings, PeriphConeRadius, PeriphRaycastMaxDistance);
                        }
                        return isHitTarget;
                        break;
                }
                return isHitTarget;
            }

            private bool raycastInConeTowardTarget(AIVisible target, int numRaycasts, int numRings, float coneRadius, float raycastMaxDistance)
            {
                // If we were fed 
                if(numRaycasts <= 1)
                {
                    return false;
                }

                // Raycast in cone.
                float angleSpacing = 360.0f / numRaycasts;
                Vector3 targetDir = (target.TargetPoint.position - m_Apex.position).normalized;
                bool aRaycastHitTarget = false;
                RaycastHit raycastHit;
                for (int j = 0; j < numRings; j++)
                {
                    float adjustedConeRadius = coneRadius * (j + 1);
                    for (int i = 0; i < numRaycasts; i++)
                    {
                        float currAngle = (i * angleSpacing) * Mathf.PI / 180.0f;
                        Vector3 rightOfMiddleRaycast = Vector3.Cross(m_Apex.up, targetDir); // Need right vector of previous single raycast shot.
                        Vector3 xOfTarget = Mathf.Cos(currAngle) * adjustedConeRadius * rightOfMiddleRaycast;
                        Vector3 yOfTarget = Mathf.Sin(currAngle) * adjustedConeRadius * m_Apex.up;
                        Vector3 zOfTarget = raycastMaxDistance * targetDir; // Mathf.Sqrt(Mathf.Pow(ConeRadius, 2) + Mathf.Pow(RaycastMaxDistance, 2));
                        Vector3 targetPos = xOfTarget + yOfTarget + zOfTarget + m_Apex.position;
                        // Shoot single raycast to see if we can see the target.
                        Vector3 dirVect = (targetPos - m_Apex.position).normalized;
                        // Do raycasts.
                        if (Physics.Raycast(m_Apex.position, dirVect, out raycastHit, raycastMaxDistance, SightRaycastLayerMask))
                        {
                            Collider collider = raycastHit.collider;
                            if (collider != null && collider.GetComponentInParent<AIVisible>() != null)
                            {
                                /* If in editor mode then draw this ray and continue so that it draws 
                                 * all the others as well for debugging purposes. */
                                #if UNITY_EDITOR
                                if (IsDrawConeRaycast)
                                {
                                    Debug.DrawRay(m_Apex.position, targetPos - m_Apex.position, RaycastSeenColor);
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
                            Debug.DrawRay(m_Apex.position, targetPos - m_Apex.position, RaycastNotSeenColor);
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

            private bool raycastOneRayTowardTarget(AIVisible target, float raycastMaxDistance)
            {
                // Shoot single raycast to see if we can see the target.
                RaycastHit raycastHit;
                Vector3 singleRaycastDir = (target.TargetPoint.position - m_Apex.position).normalized;
                if (Physics.Raycast(m_Apex.position, singleRaycastDir, out raycastHit, raycastMaxDistance, SightRaycastLayerMask))
                {
                    Collider collider = raycastHit.collider;
                    if (collider != null && collider.GetComponentInParent<AIVisible>() != null)
                    {
                        #if UNITY_EDITOR
                        // Draw raycast with not seen color to indicate target seen by AI.
                        if (IsDrawConeRaycast)
                        {
                            Debug.DrawRay(m_Apex.position, raycastMaxDistance * singleRaycastDir, RaycastSeenColor);
                        }
                        #endif
                        return true;
                    }
                }

                #if UNITY_EDITOR
                // Draw raycast with not seen color to indicate nothing being seen by AI with single raycast.
                if (IsDrawConeRaycast)
                {
                    Debug.DrawRay(m_Apex.position, raycastMaxDistance * singleRaycastDir, RaycastNotSeenColor);
                }
                #endif

                return false;
            }

#if UNITY_EDITOR
            void OnDrawGizmos()
            {
                if (IsDrawFrustum)
                {
                    Gizmos.color = DirectFOVColor;
                    float angleOfLines = m_DirectFOVHalfed;
                    Vector3 vRightView = Quaternion.AngleAxis(angleOfLines, m_Apex.transform.up) * m_Apex.transform.forward * DirectRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vRightView);
                    Vector3 vLeftView = Quaternion.AngleAxis(-angleOfLines, m_Apex.transform.up) * m_Apex.transform.forward * DirectRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vLeftView);

                    Gizmos.color = SideFOVColor;
                    angleOfLines = m_DirectFOVHalfed + m_SideFOVHalfed;
                    vRightView = Quaternion.AngleAxis(angleOfLines, m_Apex.transform.up) * m_Apex.transform.forward * SideRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vRightView);
                    vLeftView = Quaternion.AngleAxis(-angleOfLines, Apex.transform.up) * m_Apex.transform.forward * SideRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vLeftView);

                    Gizmos.color = PeriphFOVColor;
                    angleOfLines = m_DirectFOVHalfed + m_SideFOVHalfed + m_PeriphFOVHalfed;
                    vRightView = Quaternion.AngleAxis(angleOfLines, m_Apex.transform.up) * m_Apex.transform.forward * PeriphRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vRightView);
                    vLeftView = Quaternion.AngleAxis(-angleOfLines, m_Apex.transform.up) * m_Apex.transform.forward * PeriphRaycastMaxDistance;
                    Gizmos.DrawLine(m_Apex.position, m_Apex.position + vLeftView);
                }
            }
            #endif
        }; // AILineOfSightDetection class
    }; // Detection namespace
}; // AI namespace