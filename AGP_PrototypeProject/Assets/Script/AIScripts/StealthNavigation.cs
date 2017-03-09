using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Wolf;


namespace AI
{
    public class StealthNavigation : MonoBehaviour {

        [SerializeField]
        private GameObject m_StealthPosGraphObject;

        private GameObject[] m_StealthPointList;

        private StealthPosition m_CurrentStealthPos;
        private StealthPosition m_NextStealthPos;

        private GameObject m_PlayerRef;

        [SerializeField]
        private bool m_isNavigating;


        // Use this for initialization
        void Start() {

            m_StealthPointList = GameObject.FindGameObjectsWithTag("StealthPoint");
            Debug.Assert(m_StealthPointList.Length < 1, "ERROR: Need stealth points on map in order to have AI Navigate!");

            m_PlayerRef = GetComponentInParent<CompanionAISM>().Player;
            Debug.Assert(!ReferenceEquals(m_PlayerRef, null), "ERROR: Need Reference to Player!");
        }

        // Update is called once per frame
        void Update() {

        }

        public void ActivateStealthNavigation()
        {
            m_NextStealthPos = FindClosestPointToPlayer().GetComponent<StealthPosition>();

            // Set Decision Node's action to be complete
            GetComponentInParent<AIStateMachine>().CompleteCurrentActionExternal(true);
        }


        public void NavigateToNextNode()
        {
            if (m_isNavigating)
                return;

            NavMeshPath path = new NavMeshPath();
            Vector3 TargetMoveToLocation = m_NextStealthPos.transform.position;
            GetComponentInParent<NavMeshAgent>().CalculatePath(TargetMoveToLocation, path);
            WolfMoveComponent wolfMoveComp = GetComponentInParent<WolfMoveComponent>();

            if (path.corners.Length == 0)
            {
                Debug.Log("NO PATH FOUND! THIS SHOULD NOT HAPPEN!!");
            }

            if (path.corners.Length != 0)
            {
                //if (m_Corners[0] != path.corners[0])
               // {
               //     m_Corners = path.corners;
                    wolfMoveComp.Move(TargetMoveToLocation, path.corners);
              //  }
            }

            m_isNavigating = true;
        }


        //////////////////////////////////////
        // Helper Funcitons


        GameObject FindClosestPointToPlayer()
        {
            Vector3 playerLoc = m_PlayerRef.transform.position;
            GameObject closestPoint = null;
            float minDistSq = 1000000;

            for(int i = 0; i < m_StealthPointList.Length; ++i)
            {
                float distSq = Vector3.SqrMagnitude(playerLoc - m_StealthPointList[i].transform.position);
                if(distSq < minDistSq)
                {
                    closestPoint = m_StealthPointList[i];
                }

            }

            return closestPoint;
        }

        StealthPosition DetermineNextStealthPoint()
        {
            Vector3 playerLoc = m_PlayerRef.transform.position;
            StealthPosition closestPoint = null;
            float minDistSq = 1000000;

            // Find Closest neighbor to player
            for (int i = 0; i < m_CurrentStealthPos.GetNextPositions().Length; ++i)
            {
                StealthPosition sp = m_CurrentStealthPos.GetNextPositions()[i];

                float distSq = Vector3.SqrMagnitude(playerLoc - sp.transform.position);
                if (distSq < minDistSq)
                {
                    closestPoint = sp;
                }

            }

            return closestPoint;
        }
    }
}
