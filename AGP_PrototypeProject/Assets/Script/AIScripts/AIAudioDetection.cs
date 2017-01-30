using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAudioDetection : MonoBehaviour
{
    [SerializeField]
    private GameObject Target;

    [Tooltip("transform that will be used for AI ear source.")]
    [SerializeField]
    private Transform Ears;

    [Tooltip("How far this AI can hear.")]
    [SerializeField]
    private float HearingRange;

    [Tooltip("Layermask used for Audio raycasting.")]
    [SerializeField]
    private LayerMask AudioRaycastLayerMask;

    [Tooltip("Color of raycast toward target when target not audible to AI.")]
    [SerializeField]
    private Color RaycastNotAudibleColor;

    [Tooltip("Color of raycast toward target when audible to AI.")]
    [SerializeField]
    private Color RaycastAudibleColor;

    [Tooltip("Color of hearing debug sphere.")]
    [SerializeField]
    private Color HearingSphereColor;

    [Tooltip("Should we draw raycast to target.")]
    [SerializeField]
    private bool IsDrawRaycastToTarget;

    [Tooltip("Should we draw hearing range as sphere around AI.")]
    [SerializeField]
    private bool IsDrawHearingRangeSphere;

    private bool m_IsTargetAudible;
    public bool IsTargetAudible
    {
        get { return m_IsTargetAudible; }
    }

    void Awake()
    {
        if (Target == null)
        {
            Debug.LogError("Target is null and must be set.");
        }
    }

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update () {
        if(IsInLAudibleRange())
        {
            m_IsTargetAudible = true;
        }
        else
        {
            m_IsTargetAudible = false;
        }
    }

    // Shoot raycast at target to see if AI can see them.
    private bool IsInLAudibleRange()
    {
        Vector3 toTargetVect = (Target.transform.position - Ears.position);
        float distTotarget = toTargetVect.magnitude;
        if(distTotarget <= HearingRange)
        {
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        // Draws sphere to show hearing range.
        if (IsDrawHearingRangeSphere)
        {
            Gizmos.color = HearingSphereColor;
            Gizmos.DrawWireSphere(Ears.position, HearingRange);
        }

        // Draw line to target that changes if target is heard.
        if (IsDrawRaycastToTarget)
        {
            if (m_IsTargetAudible)
            {
                Debug.DrawRay(Ears.position, Target.transform.position - Ears.position, RaycastAudibleColor);
            }
            else
            {
                Debug.DrawRay(Ears.position, Target.transform.position - Ears.position, RaycastNotAudibleColor);
            }
        }
    }
}
