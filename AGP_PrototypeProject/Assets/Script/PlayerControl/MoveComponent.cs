using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Player;
using CameraController;
using AI.Detection;
using Items;
using Audio;

[RequireComponent(typeof(AudioContainer))]
public class MoveComponent : MonoBehaviour {

    [SerializeField]
    private Transform m_cam;

    [SerializeField]
    float m_MovingTurnSpeed = 360;
    [SerializeField]
    float m_StationaryTurnSpeed = 180;
    [SerializeField]
    float m_JumpPower = 11f;
    [Range(1f, 4f)]
    [SerializeField]
    float m_GravityMultiplier = 2f;
    [SerializeField]
    float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
    [SerializeField]
    float m_MoveSpeedMultiplier = 1f;
    [SerializeField]
    float m_RunSpeedMultiplier = 1.4f;
    [SerializeField]
    float m_AnimSpeedMultiplier = 1f;
    [SerializeField]
    float m_GroundCheckDistance = 0.1f;
    [SerializeField]
    float m_CrouchingToggleDelayThreshold = 0.2f;
    [SerializeField]
    float m_RunningToggleDelayThreshold = 0.2f;
    [SerializeField]
    float m_MoveSoundThreshold = .02f;
    [SerializeField]
    float m_CrouchSoundRange = 1.4f;
    [SerializeField]
    float m_WalkSoundRange = 3.0f;
    [SerializeField]
    Transform Spine;

    Rigidbody m_Rigidbody;
    Animator m_Animator;
    bool m_IsGrounded;
    float m_OrigGroundCheckDistance;
    const float k_Half = 0.5f;
    float m_TurnAmount;
    float m_ForwardAmount;
    Vector3 m_GroundNormal;
    float m_CapsuleHeight;
    Vector3 m_CapsuleCenter;
    CapsuleCollider m_Capsule;
    public bool m_Crouching;
    float m_CrouchingToggleDelay;
    bool m_Running;
    float m_RunningToggleDelay;
    float m_StrafeForward;
    float m_StrafeRight;
    bool m_Aim;
    bool m_WasAiming;
    public CameraRig m_CamRig;
    float m_jumpDeficit;
    AIAudible m_Audible;
    AudioContainer m_AudioContainer;

    void Start()
    {
        if (Camera.main)
        {
            m_cam = Camera.main.transform;
        }
        else
        {
            Debug.Log("SCENE MSISING CAMERA");
        }

        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Capsule = GetComponent<CapsuleCollider>();
        m_Audible = GetComponent<AIAudible>();
        m_AudioContainer = GetComponent<AudioContainer>();
        m_Audible.enabled = false;
        m_CapsuleHeight = m_Capsule.height;
        m_CapsuleCenter = m_Capsule.center;

        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        m_OrigGroundCheckDistance = m_GroundCheckDistance;
        m_CamRig = FindObjectOfType<CameraRig>();
    }

    public void ProcessMovement(PCActions pca)
    {
        #region GetInputsFromUserInput
        if (pca.InputPackets[(int)EnumService.InputType.LeftStickX] != null)
        {
            pca.Horizontal = pca.InputPackets[(int)EnumService.InputType.LeftStickX].Value;
            pca.StrafeRight = pca.InputPackets[(int)EnumService.InputType.LeftStickX].Value;
        }
        if (pca.InputPackets[(int)EnumService.InputType.LeftStickY] != null)
        {
            pca.Vertical = pca.InputPackets[(int)EnumService.InputType.LeftStickY].Value;
            pca.StrafeForward = pca.InputPackets[(int)EnumService.InputType.LeftStickY].Value;
        }

        if (m_cam != null)
        {
            pca.CamForward = Vector3.Scale(m_cam.forward, new Vector3(1, 0, 1)).normalized;
            pca.CamRight = m_cam.right;

            pca.Move = pca.Vertical * pca.CamForward + pca.Horizontal * pca.CamRight;
        }
        else
        {
            pca.Move = pca.Vertical * Vector3.forward + pca.Horizontal * Vector3.right;
        }

        if (pca.InputPackets[(int)EnumService.InputType.X] != null)
        {
            pca.Jump = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.X].Value);
        }

        if (pca.InputPackets[(int)EnumService.InputType.LeftStickButton] != null)
        {
            pca.Crouch = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.LeftStickButton].Value);       
        }

        if (pca.InputPackets[(int)EnumService.InputType.RightStickButton] != null)
        {
            pca.Running = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.RightStickButton].Value);
        }

        if (pca.InputPackets[(int)EnumService.InputType.LT] != null)
        {
            pca.Aim = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.LT].Value);
        }

        #endregion

        //speed mutiplier (player press a button to run or some shit)
        //TODO later

        Move(pca);
    }

    private void ProcessMovementSound(Vector3 move)
    {
        // activate audible component if moving.
        if (m_Audible != null)
        {
            if(m_Crouching)
            {
                m_Audible.SetRange(m_CrouchSoundRange);
            }
            else if(m_IsGrounded)
            {
                m_Audible.SetRange(m_WalkSoundRange);
            }

            if (move.magnitude > m_MoveSoundThreshold)
            {
                m_Audible.enabled = true;
            }
            else
            {
                m_Audible.enabled = false;
            }
        }
    }

    void Move(PCActions pca)
    {
        #region Parse PCA to private members
        Vector3 move = pca.Move;
        m_Aim = pca.Aim;
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);
        CheckGroundStatus();
        move = Vector3.ProjectOnPlane(move, m_GroundNormal);
        m_TurnAmount = Mathf.Atan2(move.x, move.z);
        m_ForwardAmount = move.z;
        if (Math.Abs(m_ForwardAmount - 0.0f) < Double.Epsilon)
        {
            m_ForwardAmount = 0.0f;
        }
        if (Math.Abs(m_TurnAmount - 0.0f) < Double.Epsilon)
        {
            m_TurnAmount = 0.0f;
        }

        if (!pca.Aim)
        {                     
            //crouching
            if (pca.Crouch && m_CrouchingToggleDelay > m_CrouchingToggleDelayThreshold)
            {               
                if (m_Crouching)
                {
                    m_Crouching = false;
                    m_Running = false;
                    m_CrouchingToggleDelay = 0.0f;
                }
                else
                {
                    m_Animator.SetFloat("Forward", 0.0f);
                    m_Animator.SetFloat("Turn", 0.0f);
                    m_Crouching = true;
                    m_Running = false;
                    m_CrouchingToggleDelay = 0.0f;
                }
            }
            m_CrouchingToggleDelay += Time.fixedDeltaTime;

            //running , same concept with crouching
            if (pca.Running && m_RunningToggleDelay > m_RunningToggleDelayThreshold)
            {
                if (m_Running)
                {
                    m_Running = false;
                    m_RunningToggleDelay = 0.0f;
                }
                else
                {
                    m_Running = true;
                    m_RunningToggleDelay = 0.0f;
                }
            }

            ApplyExtraTurnRotation();
        }
        else //this is where we go into aim mode
        {
            //just now switch
            if (m_WasAiming != m_Aim)
            {
                m_Animator.SetFloat("Forward", 0.0f);
                m_Animator.SetFloat("Turn", 0.0f);
            }
            m_WasAiming = m_Aim;
            m_RunningToggleDelay += Time.fixedDeltaTime;
            m_StrafeForward = pca.StrafeForward;
            m_StrafeRight = pca.StrafeRight;
            Vector3 forward = m_cam.TransformDirection(Vector3.forward);
            forward.y = 0.0f;
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            Quaternion newRotation = Quaternion.Slerp(GetComponent<Rigidbody>().rotation, targetRotation, 15.0f * Time.deltaTime);
            GetComponent<Rigidbody>().MoveRotation(newRotation);
        }

        #endregion

        //control and velocity handling is different when grounded and airborne:
        if (m_IsGrounded)
        {
            //HandleGroundedMovement(crouch, jump);
            HandleGroundedMovement(m_Crouching, pca.Jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        //ScaleCapsuleForCrouching(crouch);
        //ScaleCapsuleForCrouching(pca.Crouch);
        PreventStandingInLowHeadroom();

        ProcessMovementSound(move);

        // send input and other state parameters to the animator
        UpdateAnimator(move);
    }

    private void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        //Debug.Log("FORW: " + m_ForwardAmount);
        m_Animator.SetBool("Aim", m_Aim);

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer child in renderers)
        {
            if (child.GetComponent<WeaponBow>())
            {
                child.enabled = m_Aim;
                if (child.GetComponentInChildren<MeshRenderer>())
                {
                    child.GetComponentInChildren<MeshRenderer>().enabled = m_Aim;
                }
            }
        }

        if (!m_Aim)
        {           
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetBool("Crouch", m_Crouching);
            m_Animator.SetBool("OnGround", m_IsGrounded);
            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            //running
            if (m_Running)
            {
                m_Animator.speed = m_RunSpeedMultiplier;
            }
            else
            {
                m_Animator.speed = m_MoveSpeedMultiplier;
            }

            // calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
            float runCycle =
                Mathf.Repeat(
                    m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + m_RunCycleLegOffset, 1);
            float jumpLeg = (runCycle < k_Half ? 1 : -1) * m_ForwardAmount;
            if (m_IsGrounded)
            {
                m_Animator.SetFloat("JumpLeg", jumpLeg);
            }

            if (m_CamRig)
            {
                m_CamRig.Zoom(false);
            }
        }
        else
        {
            m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            m_Animator.SetFloat("StrafeForward", m_StrafeForward);
            m_Animator.SetFloat("StrafeRight", m_StrafeRight);
            if (m_CamRig)
            {
                m_CamRig.Zoom(true);
            }
            
            
        }
        /**************TODO: CHEC THIS **************/
        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        //if (m_IsGrounded && move.magnitude > 0)
        //{
        //    m_Animator.speed = m_AnimSpeedMultiplier;
        //}
        //else
        //{
        //    // don't use that while airborne
        //    m_Animator.speed = 1;
        //}
    }

    private void PreventStandingInLowHeadroom()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        m_Rigidbody.AddForce(extraGravityForce);

        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.5f;
    }

    private void ScaleCapsuleForCrouching(bool crouch)
    {
        //THIS IS TRICKY, TODO LATER
    }

    private void HandleAirborneMovement()
    {
        // apply extra gravity from multiplier:
        Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
        Vector3 toMove = transform.forward * m_ForwardAmount;
        if (m_jumpDeficit < 130.0f) { 
            transform.position += new Vector3(toMove.x / 8.0f, 0, toMove.z /8.0f);
        } else
        {
            transform.position += new Vector3(toMove.x / 11.0f, 0, toMove.z / 11.0f);
        }
        m_Rigidbody.AddForce(extraGravityForce);        
        m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.5f;

    }

    private void HandleGroundedMovement(bool crouch, bool jump)
    {
        // check whether conditions are right to allow a jump:
        if (jump && /*!crouch && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded")*/ m_IsGrounded)
        {
            // jump!
            // m_AudioContainer.PlaySound(0);
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
            m_IsGrounded = false;
            m_Animator.applyRootMotion = false;
            m_GroundCheckDistance = 0.1f;
            m_jumpDeficit = m_Rigidbody.velocity.sqrMagnitude;
        }
    }

    private void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
        transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
        {
            m_GroundNormal = hitInfo.normal;
            m_IsGrounded = true;
            m_Animator.applyRootMotion = true;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundNormal = Vector3.up;
            m_Animator.applyRootMotion = false;
        }
    }

    public void DoEndGame()
    {
        if (m_Animator)
        {
            m_Animator.SetFloat("Forward", 0f);
            m_Animator.SetFloat("Turn", 0f);
            m_Animator.Stop();
        }
    }

    public void DoGameInterruption(EnumService.GameState state)
    {
        switch (state)
        {
            case EnumService.GameState.InPauseMenu:
            case EnumService.GameState.InTutorial:
                m_Animator.speed = 0;
                m_Rigidbody.isKinematic = true;
                //Time.timeScale = 0f;
                break;
            case EnumService.GameState.InGame:
                m_Animator.speed = 1;
                m_Rigidbody.isKinematic = false;
                //Time.timeScale = 1f;
                break;
        }
    }
}
