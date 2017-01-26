using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Tooltip("Color of raycast toward player when in view of AI.")]
    [SerializeField]
    private Color RaycastNotSeenColor;

    [Tooltip("Color of raycast toward player when seen by AI.")]
    [SerializeField]
    private Color RaycastSeenColor;

    [Tooltip("Should we draw forward of frustum.")]
    [SerializeField]
    private bool IsDrawForward;

    [Tooltip("Should we draw raycast to target.")]
    [SerializeField]
    private bool IsDrawRaycastToTarget;

    [Tooltip("Should we draw frustum.")]
    [SerializeField]
    private bool IsDrawFrustum;

    [Tooltip("If checked will do basic dot products to find if enemy seen. If not, will attempt frustum check (may not be working yet).")]
    [SerializeField]
    private bool IsUsePeripheralVision;

    private Vector3[] m_FaceNormals = new Vector3[6];
    private Vector3[] m_PlanePositions = new Vector3[6];

    private bool m_IsCanSeePlayer;
    public bool IsCanSeePlayer
    {
        get { return m_IsCanSeePlayer; }
    }

    void Start()
    {
        m_FOVHalfed = FOV/2;
    }

    void Update()
    {
        if(IsInPeripherals())
        {
            if(IsInLineOfSight())
            {
                m_IsCanSeePlayer = true;
            }
            else
            {
                m_IsCanSeePlayer = false;
            }
        }
        else
        {
            m_IsCanSeePlayer = false;
        }


    }

    public bool IsAudible()
    {
        return true;
    }

    // Shoot raycast at player to see if AI can see them.
    private bool IsInLineOfSight()
    {
        RaycastHit raycastHit;
        Vector3 dirVect = (Target.transform.position - Apex.position).normalized;
        if (Physics.Raycast(Apex.position, dirVect, out raycastHit, RaycastMaxDistance, SightRaycastLayerMask))
        {
            Collider collider = raycastHit.collider;
            if(collider != null && collider.tag == "Player")
            {
                return true;
            }
        }
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
        else if(-dotCombinedWithForward == -nForward) // this is done because cosine(180) is 0 so we need this edge case check.
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

        // Draw line to target.
        Gizmos.color = Color.red;
        if (IsDrawRaycastToTarget)
        {
            if(m_IsCanSeePlayer)
            {
                Debug.DrawRay(Apex.position, Target.transform.position - Apex.position, RaycastSeenColor);
            }
            else
            {
                Debug.DrawRay(Apex.position, Target.transform.position - Apex.position, RaycastNotSeenColor);
            }
        }
    }

    /* 
     * AFTER THIS FUNCTION THESE INDICES WILL BE FILLED AS SUCH:
     * 0 - far face normal
     * 1 - near face normal
     * 2 - left face normal
     * 3 - right face normal
     * 4 - up face normal
     * 5 - down face normal
     * */
    private void CalculateFrustumPlanes()
    {
        // DO NORMAL CALCULATIONS::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // far face normal
        m_FaceNormals[0] = -Apex.forward;
        // near face normal
        m_FaceNormals[1] = Apex.forward;
        // left face normal
        Vector3 leftFaceVect = (Quaternion.AngleAxis(-m_FOVHalfed, Apex.up) * Apex.forward).normalized;
        m_FaceNormals[2] = Vector3.Cross(leftFaceVect, -Apex.up);
        // right face normal
        Vector3 rightFaceVect = (Quaternion.AngleAxis(m_FOVHalfed, Apex.up) * Apex.forward).normalized;
        m_FaceNormals[3] = Vector3.Cross(rightFaceVect, Apex.up);
        // up face normal
        Vector3 upFaceVect = (Quaternion.AngleAxis(-m_FOVHalfed / Aspect, Apex.right) * Apex.forward).normalized;
        m_FaceNormals[4] = Vector3.Cross(upFaceVect, -Apex.right);
        // down face normal
        Vector3 downFaceVect = (Quaternion.AngleAxis(m_FOVHalfed / Aspect, Apex.right) * Apex.forward).normalized;
        m_FaceNormals[5] = Vector3.Cross(downFaceVect, Apex.right);

        // DO PLANE CALCULATIONS:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // Draw far face normal.
        m_PlanePositions[0] = Apex.forward * MaxRange + Apex.position;
        Debug.DrawRay(m_PlanePositions[0], m_FaceNormals[0], PlaneNormalColor);

        // Draw near face normal.
        m_PlanePositions[1] = Apex.forward * MinRange + Apex.position;
        Debug.DrawRay(m_PlanePositions[1], m_FaceNormals[1], PlaneNormalColor);

        // find dist to center of frustum used for all plane normal calculation proceeding.
        float distToCenterFrustum = MinRange + ((MaxRange - MinRange) / 2.0f);
        Vector3 vectToCenterFrustum = Apex.forward * distToCenterFrustum;
        Debug.DrawRay(Apex.position, vectToCenterFrustum);

        // Draw normal of left plane
        float dotProdResult = Vector3.Dot(vectToCenterFrustum, leftFaceVect);
        m_PlanePositions[2] = leftFaceVect * dotProdResult;
        Debug.DrawRay(Apex.position, m_PlanePositions[2]);
        Debug.DrawRay(m_PlanePositions[2], m_FaceNormals[2], PlaneNormalColor);

        // Draw normal of right plane
        dotProdResult = Vector3.Dot(vectToCenterFrustum, rightFaceVect);
        m_PlanePositions[3] = rightFaceVect * dotProdResult;
        Debug.DrawRay(Apex.position, m_PlanePositions[3]);
        Debug.DrawRay(m_PlanePositions[3], m_FaceNormals[3], PlaneNormalColor);

        // Draw normal of up plane
        dotProdResult = Vector3.Dot(vectToCenterFrustum, upFaceVect);
        m_PlanePositions[4] = upFaceVect * dotProdResult;
        Debug.DrawRay(Apex.position, m_PlanePositions[4]);
        Debug.DrawRay(m_PlanePositions[4], m_FaceNormals[4], PlaneNormalColor);

        // Draw normal of down plane
        dotProdResult = Vector3.Dot(vectToCenterFrustum, downFaceVect);
        m_PlanePositions[5] = downFaceVect * dotProdResult;
        Debug.DrawRay(Apex.position, m_PlanePositions[5]);
        Debug.DrawRay(m_PlanePositions[5], m_FaceNormals[5], PlaneNormalColor);
    }

    // Check to see if within peripherals.
    private bool IsInFrustumUsingPlanes()
    {
        for (int i = 0; i < m_FaceNormals.Length; i++)
        {
            float lengthOfProjection = Vector3.Dot(Target.transform.position, m_FaceNormals[i]);
            Vector3 normalProjectedVect = lengthOfProjection * m_FaceNormals[i];
            Vector3 fromPlaneToTarget = normalProjectedVect - m_PlanePositions[i];
            if (fromPlaneToTarget.magnitude < 0)
            {
                return false;
            }
        }
        return true;
    }
}
