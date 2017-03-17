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


        //bool tempSelfNav = true;
        

        [SerializeField]
        private GameObject m_StealthPosGraphObject;

        private GameObject[] m_StealthPointList;

        private GameObject m_CurrentStealthPos;
        public GameObject CurrentStealthPos { get { return m_CurrentStealthPos; } }

        private GameObject m_NextStealthPos;
        public GameObject NextStealthPos {
            get {
                if (!m_FindOwnPath)
                    return m_NextStealthPos;
                else if (m_PathToDestination.Count > 0)
                    return m_PathToDestination.Peek();
                else
                    return null;
            }
        }


        private GameObject m_PlayerRef;

        private ActionZone m_CurrentActionZone;
        public ActionZone CurrentActionZone
        {
            get { return m_CurrentActionZone; }
            set { m_CurrentActionZone = value; }
        }

        private Vector3 m_FinalDestination;
        private GameObject m_ClosestSPToFinalDest;

        [SerializeField]
        private bool m_isNavigating = false;

        [SerializeField]
        float m_MinDistToPointSq = 2.0f;
        [SerializeField]
        float m_MinDistEnemyPathAvoidance;

        [SerializeField]
        [Tooltip("Boundary color of debug lines drawn for stealth path")]
        private Color StealthPathColor;


        [SerializeField]
        private Stack<GameObject> m_PathToDestination;

        [SerializeField]
        private bool m_FindOwnPath;
        public bool FindOwnPath
        {
            get { return m_FindOwnPath; }
            set { m_FindOwnPath = value; }
        }

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

        void Awake()
        {
            //if (GameCritical.GameController.Instance.BondManager.BondStatus >= 50)
            //    m_FindOwnPath = false;
            //else
            //    m_FindOwnPath = true;
        }

        // Update is called once per frame
        void Update() {

        }

        public void ActivateStealthNavigation()
        {
            // Get the correct stealth points from current ActionZone
            m_CurrentActionZone = GameCritical.GameController.Instance.CurrentActionZone;
            m_StealthPointList = m_CurrentActionZone.StealthPointList;

            //m_NextStealthPos = FindClosestPointToPlayer();
            //m_CurrentStealthPos =  m_NextStealthPos;

            // Set which stealth strategy to use
            if (GameCritical.GameController.Instance.BondManager.BondStatus >= 50)
            {
                m_FindOwnPath = false;
                m_NextStealthPos = FindClosestPointToPlayer();
                m_CurrentStealthPos = m_NextStealthPos;
            }
            else if (m_CurrentActionZone)
            {
                m_FindOwnPath = true;
                m_ClosestSPToFinalDest = m_CurrentActionZone.FinalStealthPos;
                m_NextStealthPos = FindClosestPointToWolf();
                m_CurrentStealthPos = m_NextStealthPos;
                CalculateBestPathToFinalDestination();
            }
            else
            {
                Debug.Assert(false, "Not sure what to do for stealth Nav!");
            }


            //if (GetComponent<CompanionAISM>().CurrentCommand == WolfCommand.GOTO)
            //    m_FindOwnPath = true;

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
                //wolfMoveComp.Move(TargetMoveToLocation, path.corners);
                NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
                StartCoroutine(wolfMoveComp.Move(TargetMoveToLocation, path.corners, navAgent));
                m_isNavigating = true;
            }

            
        }

        

        public bool IsAtNextNode()
        {
            Vector3 wolfPos = transform.position;

            float distToNextSq = (wolfPos - m_NextStealthPos.transform.position).sqrMagnitude;

            if(distToNextSq < m_MinDistToPointSq)
            {
                m_CurrentStealthPos = m_NextStealthPos;
                m_isNavigating = false;
                return true;
            }

            return false;
        }

        public void ExecuteStealthGoToCommand(Vector3 goToLocation)
        {
            m_NextStealthPos = FindClosestPointToWolf();
            m_CurrentStealthPos = m_NextStealthPos;

            SetStealthToDestination(goToLocation);
            CalculateBestPathToFinalDestination();
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

        GameObject FindClosestPointToWolf()
        {
            Vector3 wolfLoc = gameObject.transform.position;
            GameObject closestPoint = null;

            float minDistSq = 1000000;

            for (int i = 0; i < m_StealthPointList.Length; ++i)
            {
                float distSq = Vector3.SqrMagnitude(wolfLoc - m_StealthPointList[i].transform.position);
                if (distSq < minDistSq)
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
                //Debug.Log("No Nodes for stealth path!");
            }
        }

        public bool IsPathSafeToNext() // Ignore enemy FOV for now
        {
            GameCritical.GameController gameControl = GameCritical.GameController.Instance;
            if (!gameControl || !gameControl.CurrentActionZone || gameControl.CurrentActionZone.GetNumEnemiesAlive() == 0)
                return true;

            // Get Wolf data
            Vector3 WolfPos = transform.position;
            Vector3 nextDir = m_NextStealthPos.transform.position - WolfPos;
            nextDir.Normalize();

            Vector3 intersectPt; // So we dont need to keep reallocating
            GameObject Enemy;
            int numEnemies = gameControl.CurrentActionZone.GetNumEnemiesAlive();
            for (int i = 0; i < numEnemies; ++i)
            {
                Enemy = gameControl.CurrentActionZone.GetEnemy(i);

                if (!Enemy)
                    continue;

                // Check Forward of Enemy
                Vector3 EnemyPos = Enemy.transform.position;
                Vector3 EnemyFaceDir = Enemy.transform.forward;
                //EnemyFaceDir.Normalize();

                if (DoRaysIntersect(WolfPos, nextDir, EnemyPos, EnemyFaceDir))
                {
                    intersectPt = GetRayIntersectionPt(WolfPos, nextDir, EnemyPos, EnemyFaceDir);
                    float distToNextSq = (m_NextStealthPos.transform.position - WolfPos).sqrMagnitude;
                    float distToIntersectSq = (intersectPt - WolfPos).sqrMagnitude;

                    float dotFaceDir = Vector3.Dot(nextDir, EnemyFaceDir);
                    dotFaceDir = Mathf.Abs(dotFaceDir);

                    if (distToNextSq > distToIntersectSq && dotFaceDir < 0.95)
                    {
                        return false;
                    }
                }

                // Make sure that there is enough distance in the path to not run into the Enemies
                {
                    intersectPt = GetRayIntersectionPt(WolfPos, nextDir, EnemyPos, EnemyFaceDir);
                    float distToEnemySq = (intersectPt - EnemyPos).sqrMagnitude;
                    if (distToEnemySq < m_MinDistEnemyPathAvoidance * m_MinDistEnemyPathAvoidance)
                    {
                        return false;
                    }
                }

                // Check Peripherals to see if my path would be seen by them
                Transform enemyTransform = Enemy.transform;
                Vector3 peripheralRight = Enemy.GetComponent<Detection.AILineOfSightDetection>().GetFOVVector(enemyTransform.forward,
                                                                                                              enemyTransform.up,
                                                                                                              Detection.AILineOfSightDetection.FOV_REGION.PERIPHERAL,
                                                                                                              true, true);
                Vector3 peripheralLeft = Enemy.GetComponent<Detection.AILineOfSightDetection>().GetFOVVector(enemyTransform.forward,
                                                                                                             enemyTransform.up,
                                                                                                             Detection.AILineOfSightDetection.FOV_REGION.PERIPHERAL,
                                                                                                             false, true);
                // If my path does not intersect with the peripherals, it is safe
                float rightPeriphTime = 0;
                float leftPeriphTime = 0;
                if (!DoRaysIntersect(WolfPos, nextDir, EnemyPos, peripheralRight, ref rightPeriphTime) && !DoRaysIntersect(WolfPos, nextDir, EnemyPos, peripheralRight, ref leftPeriphTime))
                {
                    intersectPt = GetRayIntersectionPt(WolfPos, nextDir, EnemyPos, EnemyFaceDir);
                    float distToEnemySq = (intersectPt - EnemyPos).sqrMagnitude;
                    if (distToEnemySq < m_MinDistEnemyPathAvoidance * m_MinDistEnemyPathAvoidance)
                    {
                        return false;
                    }

                    return true;
                }

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

            // Find stealth point closest to destination
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
            //m_ClosestSPToFinalDest = GetComponent<CompanionAISM>().StealthDestination.gameObject;
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
