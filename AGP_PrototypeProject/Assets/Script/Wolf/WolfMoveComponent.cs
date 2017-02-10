using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolf {
    public class WolfMoveComponent : MonoBehaviour {
        private Animator m_Animator;
        private Vector3 m_GroundNormal;
        private float m_GroundCheckDistance = 0.5f;
        private bool m_IsGrounded = false;
        private float m_TurnAmount;
        private float m_ForwardAmount;
        private Vector3 m_MovementAxis;
        public float m_MaxDist = 0.2f;
        public float m_MaxRotation = 90;

        private Vector3 m_TargetPos;
        private Vector3[] m_Path;
        private float currRotateRate;
        private float m_CurRotRate;
        
        void Start()
        {
            m_Animator = GetComponent<Animator>();
        }

        public void Move(Vector3 targetPos, Vector3[] path)
        {
            m_TargetPos = targetPos;
            m_Path = path;
            Vector3 nextNode = path[1];

            Vector3 dir = nextNode - transform.position;
            dir.Normalize();

            Vector3 cross = Vector3.zero ;

            if (transform.forward != dir)
            {
                cross = Vector3.Cross(transform.forward, dir);
                float angle = Vector3.Angle(transform.forward, dir);
                float rate = angle / m_MaxRotation;
                
                Mathf.Clamp(rate, 0, 3);
                
                if (Vector3.Dot(cross, Vector3.up) > 0)
                {
                    //Debug.Log("Rate: " + rate);
                    rate = Mathf.Lerp(m_CurRotRate, rate, Time.deltaTime*15);
                    m_CurRotRate = rate;
                    m_Animator.SetFloat("Horizontal", rate);
                }
                else
                {
                    rate *= -1;
                    //Debug.Log("Rate: " + rate);
                    rate = Mathf.Lerp(m_CurRotRate, rate, Time.deltaTime*15);
                    m_CurRotRate = rate;
                    m_Animator.SetFloat("Horizontal", rate);
                }
            }
            else
            {
                m_Animator.SetFloat("Horizontal", 0);
            }
            Debug.DrawRay(transform.position, dir * 10, Color.red);
            Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
            Debug.DrawRay(transform.position, cross * 10, Color.yellow);


            float dist = Vector3.Distance(transform.position, m_TargetPos);
            
            dist = dist / 3;            
            Mathf.Clamp(dist, 0, 3);
            m_Animator.SetFloat("Vertical", dist);           
        }

        public void Stop()
        {
            m_Animator.SetFloat("Vertical", 0);
            m_Animator.SetFloat("Horizontal", 0);
        }

        //move this to utility later plz
        bool Equal(Vector3[] a1, Vector3[] a2)
        {
            if (a1.Length != a2.Length)
            {
                return false;
            }
            for (int i=0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
