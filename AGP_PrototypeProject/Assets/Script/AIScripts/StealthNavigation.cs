using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Wolf;

namespace AI
{
    /// <summary>
    /// 
    /// This class is just used for internal navigation to find the best path to a specific node
    /// 
    /// </summary>
    //public class StealthPathNode
    //{
    //    public GameObject m_StealthPos;
    //    public bool m_Visited;
    //}

    public class StealthNavigation : MonoBehaviour {


        bool tempSelfNav = true;

        [SerializeField]
        private GameObject m_StealthPosGraphObject;

        private GameObject[] m_StealthPointList;

        private GameObject m_CurrentStealthPos;
        private GameObject m_NextStealthPos;
        public GameObject NextStealthPos {
            get {
                if (!tempSelfNav)
                    return m_NextStealthPos;
                else if (m_PathToDestination.Count > 0)
                    return m_PathToDestination.Peek();
                else
                    return null;
            }
        }


        private GameObject m_PlayerRef;

        private ActionZone m_CurrentActionZone;

        private Vector3 m_FinalDestination;
        private GameObject m_ClosestSPToFinalDest;

        [SerializeField]
        private bool m_isNavigating = false;

        [SerializeField]
        [Tooltip("Boundary color of debug lines drawn for stealth path")]
        private Color StealthPathColor;


        [SerializeField]
        private Stack<GameObject> m_PathToDestination;


        // Use this for initialization
        void Start() {
            
            m_PlayerRef = GetComponentInParent<CompanionAISM>().Player;
            Debug.Assert(!ReferenceEquals(m_PlayerRef, null), "ERROR: Need Reference to Player!");

            // Get all child game objects
            int numChild = m_StealthPosGraphObject.transform.childCount;
            m_StealthPointList = new GameObject[numChild];

            for (int i = 0; i < numChild; ++i)
            {
                m_StealthPointList[i] = m_StealthPosGraphObject.transform.GetChild(i).gameObject;
            }

            Debug.Assert(m_StealthPointList.Length > 1, "ERROR: Need stealth points on map in order to have AI Navigate!");

            m_PathToDestination = new Stack<GameObject>();
        }

        // Update is called once per frame
        void Update() {

        }

        public void ActivateStealthNavigation()
        {
            m_NextStealthPos = FindClosestPointToPlayer();//.GetComponentInParent<StealthPosition>();
            m_CurrentStealthPos =  m_NextStealthPos;

            // Test "Go To" for stealth - default to go to last node in the path in test level
            SetStealthToDestination(Vector3.zero);
            CalculateBestPathToFinalDestination();
        }


        public void NavigateToNextNode()
        {
            // if (m_isNavigating)
            //   return;

            //DetermineNextStealthPointToPlayer();
            //DetermineNextStealthPointInPath();

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
                    wolfMoveComp.Move(TargetMoveToLocation, path.corners);
                    m_isNavigating = true;
            }

            
        }

        [SerializeField]
        float MinDistToPointSq = 2.0f;

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

        public void DetermineNextStealthPointToPlayer()
        {
            Vector3 playerLoc = m_PlayerRef.transform.position;
            Vector3 playerFaceDir = m_PlayerRef.transform.forward;
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
                //bool isPlayerFacingNode = 
                if (distSq < minDistSq)
                {
                    closestPoint = sp;
                    minDistSq = distSq;
                }

            }

            if(!ReferenceEquals(closestPoint, null))
                m_NextStealthPos = closestPoint.gameObject;
        }

        public void DetermineNextStealthPointInPath()
        {
            if(m_PathToDestination.Count != 0 && IsAtNextNode())
                m_NextStealthPos = m_PathToDestination.Pop();
            else
            {
                Debug.Log("No Nodes for stealth path!");
            }
        }

        public bool IsPathSafeToNext() // Ignore enemy FOV for now
        {
            // Find Intersection of EnemyDir and my desired direction
            //Vector3 EnemyPos = GetComponent<CompanionAISM>().Enemy.transform.position;
            //Vector3 EnemyFaceDir = GetComponent<CompanionAISM>().Enemy.transform.forward;
            //EnemyFaceDir.Normalize();

            //Vector3 WolfPos = transform.position;
            //Vector3 nextDir = m_NextStealthPos.transform.position - WolfPos;
            //nextDir.Normalize();

            //Vector3 crossDir = Vector3.Cross(EnemyFaceDir, nextDir);
            //Vector3 leftOfDot = Vector3.Cross((WolfPos - EnemyPos), nextDir);

            //float timeDirsIntersect = Vector3.Dot(leftOfDot, crossDir) / crossDir.sqrMagnitude;

            //// If Enemy is walking in opposite direction, path is safe
            //if (timeDirsIntersect < 0)
            //    return true;

            //// If my destination is before the intersection, it is safe
            //Vector3 rayIntersection = EnemyPos + EnemyFaceDir * timeDirsIntersect;
            //float distSqToIntersect = (rayIntersection - WolfPos).sqrMagnitude;
            //float distSqToStealthPoint = (m_NextStealthPos.transform.position - WolfPos).sqrMagnitude;

            //if (distSqToIntersect > distSqToStealthPoint)
            //{
            //    Debug.Log("Path Safe because destination is closer than ray intersection: dstToSP = " + distSqToStealthPoint);
            //    return true;
            //}

            //return false;

            // Get Wolf data
            Vector3 WolfPos = transform.position;
            Vector3 nextDir = m_NextStealthPos.transform.position - WolfPos;
            nextDir.Normalize();

            // Check Forward of Enemy
            Vector3 EnemyPos = GetComponent<CompanionAISM>().Enemy.transform.position;
            Vector3 EnemyFaceDir = GetComponent<CompanionAISM>().Enemy.transform.forward;
            EnemyFaceDir.Normalize();

            if (DoRaysIntersect(WolfPos, nextDir, EnemyPos, EnemyFaceDir))
                return false;

            //return true;

            // Check Peripherals to see if my path would be seen by them
            GameObject enemy = GetComponent<CompanionAISM>().Enemy;
            Transform enemyTransform = enemy.transform;
            Vector3 peripheralRight = enemy.GetComponent<Detection.AILineOfSightDetection>().GetFOVVector(enemyTransform.forward,
                                                                                                          enemyTransform.up,
                                                                                                          Detection.AILineOfSightDetection.FOV_REGION.PERIPHERAL,
                                                                                                          true, true);
            Vector3 peripheralLeft = enemy.GetComponent<Detection.AILineOfSightDetection>().GetFOVVector(enemyTransform.forward,
                                                                                                         enemyTransform.up,
                                                                                                         Detection.AILineOfSightDetection.FOV_REGION.PERIPHERAL,
                                                                                                         false, true);
            // If my path does not intersect with the peripherals, it is safe
            float rightPeriphTime = 0;
            float leftPeriphTime = 0;
            if (!DoRaysIntersect(WolfPos, nextDir, EnemyPos, peripheralRight, ref rightPeriphTime) && !DoRaysIntersect(WolfPos, nextDir, EnemyPos, peripheralRight, ref leftPeriphTime))
                return true;

            if (rightPeriphTime != 0)
            {
                Vector3 rayIntersection = EnemyPos + peripheralRight * rightPeriphTime;
                float distSqToIntersect = (rayIntersection - WolfPos).sqrMagnitude;
                float distSqToStealthPoint = (m_NextStealthPos.transform.position - WolfPos).sqrMagnitude;
                if (distSqToIntersect < distSqToStealthPoint)
                {
                    return false;
                }
            }

            if (leftPeriphTime != 0)
            {
                Vector3 rayIntersection = EnemyPos + peripheralLeft * leftPeriphTime;
                float distSqToIntersect = (rayIntersection - WolfPos).sqrMagnitude;
                float distSqToStealthPoint = (m_NextStealthPos.transform.position - WolfPos).sqrMagnitude;
                if (distSqToIntersect < distSqToStealthPoint)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DoRaysIntersect(Vector3 pos1, Vector3 dir1, Vector3 pos2, Vector3 dir2)
        {
            Vector3 crossDir = Vector3.Cross(dir2, dir1);
            Vector3 leftOfDot = Vector3.Cross((pos1 - pos2), dir1);

            float timeDirsIntersect = Vector3.Dot(leftOfDot, crossDir) / crossDir.sqrMagnitude;

            if (timeDirsIntersect > 0)
                return true;

            return false;
        }

        private bool DoRaysIntersect(Vector3 pos1, Vector3 dir1, Vector3 pos2, Vector3 dir2, ref float intersectTimeOut)
        {
            Vector3 crossDir = Vector3.Cross(dir2, dir1);
            Vector3 leftOfDot = Vector3.Cross((pos1 - pos2), dir1);

            intersectTimeOut = Vector3.Dot(leftOfDot, crossDir) / crossDir.sqrMagnitude;

            if (intersectTimeOut > 0)
                return true;

            return false;
        }

        public Vector3 GetRayIntersectionPt(Vector3 pos1, Vector3 dir1, Vector3 pos2, Vector3 dir2) {

            Vector3 crossDir = Vector3.Cross(dir2, dir1);
            Vector3 leftOfDot = Vector3.Cross((pos1 - pos2), dir1);

            float timeDirsIntersect = Vector3.Dot(leftOfDot, crossDir) / crossDir.sqrMagnitude;

            return (pos2 + dir2 * timeDirsIntersect);

        }

        public void SetStealthToDestination(Vector3 goToLocation)
        {
            m_FinalDestination = goToLocation;

            float minDist = 1000000;

            for(int i = 0; i < m_StealthPointList.Length; ++i)
            {
                float distToPointSq = (m_StealthPointList[i].transform.position - goToLocation).sqrMagnitude;
                if(distToPointSq < minDist)
                {
                    minDist = distToPointSq;
                    m_ClosestSPToFinalDest = m_StealthPointList[i];
                }
            }

            // TEMP FOR TEST - REMOVE THIS
            m_ClosestSPToFinalDest = GetComponent<CompanionAISM>().StealthDestination.gameObject;
        }

        private void SetAllStealthPosVisited(bool isVisited)
        {
            for(int i = 0; i < m_StealthPointList.Length; ++i)
            {
                m_StealthPointList[i].GetComponent<StealthPosition>().Visited = false;
            }
        }

        private void CalculateBestPathToFinalDestination()
        {
            SetAllStealthPosVisited(false);

            if (!m_ClosestSPToFinalDest)
            {
                Debug.Log("Error: StealthNavigation: Could not find path to final destination because there is no final destination!");
                return;
            }


            // Just use BFS for now
            Queue<GameObject> emptyQueue = new Queue<GameObject>();
            HashSet<GameObject> emptySet = new HashSet<GameObject>();

            m_CurrentStealthPos.GetComponent<StealthPosition>().ParentForPath = null;
            m_CurrentStealthPos.GetComponent<StealthPosition>().Visited = true;
            emptySet.Add(m_CurrentStealthPos);
            emptyQueue.Enqueue(m_CurrentStealthPos);

            string curName;

            while(emptyQueue.Count != 0)
            {
                GameObject current = emptyQueue.Dequeue();
                curName = current.GetComponent<StealthPosition>().m_name;

                if(current == m_ClosestSPToFinalDest)
                {
                    m_PathToDestination.Clear();
                    m_PathToDestination.Push(current);

                    // Fill actual path with parents
                    StealthPosition parent = current.GetComponent<StealthPosition>().ParentForPath;
                    while (parent != null)
                    {
                        m_PathToDestination.Push(parent.gameObject);
                        parent = parent.ParentForPath;
                    }

                    return;
                }

                StealthPosition[] neighbors = current.GetComponent<StealthPosition>().GetNextPositions();
                for (int i = 0; i < neighbors.Length; ++i)
                {
                    if (!emptySet.Contains(neighbors[i].gameObject))
                    {
                        emptySet.Add(neighbors[i].gameObject);
                        neighbors[i].ParentForPath = current.GetComponent<StealthPosition>();
                        emptyQueue.Enqueue(neighbors[i].gameObject);
                        neighbors[i].Visited = true;
                    }
                }
            }

        }
        
        public bool IsStealthPathComplete()
        {
            if (m_PathToDestination.Count == 0)
                return true;

            return false;
        }

        //void OnDrawGizmos()
        //{
        //    // Draw stealth path
        //    // Gizmos.DrawIcon(this.transform.position + Vector3.up * PatrolIconHeight, "patrollingIcon.png");
        //    if (m_StealthPointList != null)
        //   {
        //        for (int i = 0; i < m_StealthPointList.Length; ++i)
        //        {
        //            GameObject currWaypoint = m_StealthPointList[i];
        //            for (int k = 0; k < currWaypoint.GetComponent<StealthPosition>().GetNextPositions().Length; ++k)
        //            {
        //                StealthPosition nextWaypoint = currWaypoint.GetComponent<StealthPosition>().GetNextPositions()[k];
        //                Gizmos.color = StealthPathColor;
        //                Gizmos.DrawLine(currWaypoint.transform.position, nextWaypoint.gameObject.transform.position);
        //            }
        //        }
        //    }
        //}

    }
}
