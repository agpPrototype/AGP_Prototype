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

        private GameObject m_CurrentStealthPos;
        private GameObject m_NextStealthPos;

        private GameObject m_PlayerRef;

        [SerializeField]
        private bool m_isNavigating = false;


        // Use this for initialization
        void Start() {
            
            m_PlayerRef = GetComponentInParent<CompanionAISM>().Player;
            Debug.Assert(!ReferenceEquals(m_PlayerRef, null), "ERROR: Need Reference to Player!");
        }

        // Update is called once per frame
        void Update() {

        }

        public void ActivateStealthNavigation()
        {
            // Get all child game objects
            int numChild = m_StealthPosGraphObject.transform.childCount;
            m_StealthPointList = new GameObject[numChild];

            for (int i = 0; i < numChild; ++i)
            {
                m_StealthPointList[i] = m_StealthPosGraphObject.transform.GetChild(i).gameObject;
            }

            Debug.Assert(m_StealthPointList.Length > 1, "ERROR: Need stealth points on map in order to have AI Navigate!");


            m_NextStealthPos = FindClosestPointToPlayer();//.GetComponentInParent<StealthPosition>();
            m_CurrentStealthPos =  m_NextStealthPos;

            // Set Decision Node's action to be complete
            //GetComponentInParent<AIStateMachine>().CompleteCurrentActionExternal(true);
        }


        public void NavigateToNextNode()
        {
           // if (m_isNavigating)
             //   return;

            DetermineNextStealthPoint();

            NavMeshPath path = new NavMeshPath();
            Vector3 TargetMoveToLocation = m_NextStealthPos.transform.position;
            GetComponentInParent<NavMeshAgent>().CalculatePath(TargetMoveToLocation, path);
            WolfMoveComponent wolfMoveComp = GetComponentInParent<WolfMoveComponent>();

            if (path.corners.Length == 0)
            {
                Debug.Log("NO PATH FOUND! THIS SHOULD NOT HAPPEN!!");
            }

            if (path.corners.Length != 0 && !IsAtNextNode())
            {
                //if (m_Corners[0] != path.corners[0])
               // {
               //     m_Corners = path.corners;
                    wolfMoveComp.Move(TargetMoveToLocation, path.corners);
                    m_isNavigating = true;
                //  }
            }

            
        }

        static float MinDistToPointSq = 4.0f;

        public bool IsAtNextNode()
        {
            Vector3 wolfPos = transform.position;

            float distToNextSq = (wolfPos - m_NextStealthPos.transform.position).sqrMagnitude;

            if(distToNextSq < MinDistToPointSq)
            {
                m_CurrentStealthPos = m_NextStealthPos;
                m_isNavigating = false;
                return true;
            }

            return false;
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
                    minDistSq = distSq;
                }

            }

            return closestPoint;
        }

        void DetermineNextStealthPoint()
        {
            Vector3 playerLoc = m_PlayerRef.transform.position;
            StealthPosition closestPoint = null;

            Vector3 wolfPos = transform.position;
            float minDistSq = (m_CurrentStealthPos.transform.position - playerLoc).sqrMagnitude;

            // Find Closest neighbor to player
            int numPos = m_CurrentStealthPos.GetComponent<StealthPosition>().GetNextPositions().Length;
            StealthPosition[] allSP = m_CurrentStealthPos.GetComponent<StealthPosition>().GetNextPositions();

            for (int i = 0; i < numPos; ++i)
            {
                StealthPosition sp = allSP[i];

                float distSq = Vector3.SqrMagnitude(playerLoc - sp.transform.position);
                if (distSq < minDistSq)
                {
                    closestPoint = sp;
                    minDistSq = distSq;
                }

            }

            if(!ReferenceEquals(closestPoint, null))
                m_NextStealthPos = closestPoint.gameObject;
        }

    }
}
