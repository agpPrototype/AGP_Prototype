using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Climb
{
    [ExecuteInEditMode]
    public class CPConnectionHandler : MonoBehaviour
    {
        [SerializeField]
        private float m_MaxDist = 2.5f;
        [SerializeField]
        private float m_JumpMin = 1.0f;
        [SerializeField]
        private float m_NeighborAngleThreshold = 20.0f;
        [SerializeField]
        private bool m_ConnectClimbPoints;
        [SerializeField]
        private bool m_ResetClimbPoints;
        

        private List<ClimbPoint> m_AllPoints = new List<ClimbPoint>();
        private Vector3[] m_AllDirs = new Vector3[8];

        void Update()
        {
            if (m_ConnectClimbPoints)
            {
                Clear();
                CreateDirs();
                GetClimbPoints();            
                ConnectAllClimbPoints();
                m_ConnectClimbPoints = false;
            }

            if (m_ResetClimbPoints)
            {
                Clear();
                m_ResetClimbPoints = false;
            }
        }
        
        void CreateDirs()
        {
            //m_AllDirs[0] = new Vector3(1, 0, 0);
            //m_AllDirs[1] = new Vector3(-1, 0, 0);
            //m_AllDirs[2] = new Vector3(0, 1, 0);
            //m_AllDirs[3] = new Vector3(0, -1, 0);
            //m_AllDirs[4] = new Vector3(-1, -1, 0);
            //m_AllDirs[5] = new Vector3(1, 1, 0);
            //m_AllDirs[6] = new Vector3(1, -1, 0);
            //m_AllDirs[7] = new Vector3(-1, 1, 0);

            m_AllDirs[0] = new Vector3(0, 0, 1);
            m_AllDirs[1] = new Vector3(0, 0, -1);
            m_AllDirs[2] = new Vector3(0, 1, 0);
            m_AllDirs[3] = new Vector3(0, -1, 0);
            m_AllDirs[4] = new Vector3(0, -1, -1);
            m_AllDirs[5] = new Vector3(0, 1, 1);
            m_AllDirs[6] = new Vector3(0, -1, 1);
            m_AllDirs[7] = new Vector3(0, 1, -1);
        }

        void Clear()
        {
            for (int i=0; i < m_AllPoints.Count; i++)
            {
                m_AllPoints[i].ClearNeighbors();
            }
            m_AllPoints.Clear();
        }
        
        void GetClimbPoints()
        {
            ClimbPoint[] points = FindObjectsOfType<ClimbPoint>();
            m_AllPoints.AddRange(points);
        }

        void ConnectAllClimbPoints()
        {
            for (int i= 0; i < m_AllPoints.Count; i++)
            {
                ClimbPoint curPoint = m_AllPoints[i];
                //check 8 directions
                for (int j=0; j < m_AllDirs.Length; j++)
                {
                    ClimbPoint potentialNeighbor = GetPotentialNeighborInDir(m_AllDirs[j], curPoint);

                    if (potentialNeighbor)
                    {
                        float dist = Vector3.Distance(potentialNeighbor.transform.position, curPoint.transform.position);
                        if (dist < m_MaxDist)
                        {
                            //skip diagonal jumping cuz i heard animation for this is a bitch?
                            if (Mathf.Abs(m_AllDirs[j].y) > 0 &&
                                Mathf.Abs(m_AllDirs[j].x) > 0)
                            {
                                if (Vector3.Distance(curPoint.transform.position, potentialNeighbor.transform.position) > m_JumpMin)
                                {
                                    continue;
                                }
                            }

                            //curPoint.AddNeighbor(potentialNeighbor, m_AllDirs[j], dist, m_JumpMin);
                            AddNeighbor(curPoint, potentialNeighbor, m_AllDirs[j], dist, m_JumpMin);
                            
                        }
                    }

                }
            }
        }

        ClimbPoint GetPotentialNeighborInDir(Vector3 dir, ClimbPoint curPoint)
        {
            //List<ClimbPoint> potentialNeighbors = new List<ClimbPoint>();
            float minDist = Mathf.Infinity;
            ClimbPoint toRet = null;
            for (int i =0; i < m_AllPoints.Count; i++)
            {
                ClimbPoint targetPoint = m_AllPoints[i];
                //same point, skip
                if ((GameObject.ReferenceEquals(targetPoint, curPoint)))
                {
                    continue;
                }

                Vector3 targetDir = targetPoint.transform.position - curPoint.transform.position;
                Vector3 objectSpaceTargetDir = curPoint.transform.InverseTransformDirection(targetDir);

                bool isPossibleDirection = ValidateDirection(dir, objectSpaceTargetDir);
                
                //now that the point is valid in the direction, check if it's the closet point in that direction
                if (isPossibleDirection)
                {
                    float dist = Vector3.Distance(targetPoint.transform.position, curPoint.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        toRet = targetPoint;
                    }
                }
            }

            return toRet;
        }

        bool ValidateDirection(Vector3 dir, Vector3 targetDir)
        {
            //get angles relative to the curPoint
            //float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            //float targetAngle = Mathf.Atan2(targetDir.x, targetDir.y) * Mathf.Rad2Deg;

            //if (targetAngle < angle + m_NeighborAngleThreshold && targetAngle > angle - m_NeighborAngleThreshold)
            //{
            //    return true;
            //}

            //return false;
            dir.Normalize();
            targetDir.Normalize();
            float dotProd = Vector3.Dot(targetDir, dir);
            if (dotProd > Mathf.Cos(m_NeighborAngleThreshold * Mathf.Deg2Rad))
            {
                return true;
            }
            return false;
        }

        public void AddNeighbor(ClimbPoint from, ClimbPoint neighborPoint, Vector3 dir, float dist, float jumpMin)
        {
            bool alreadyNeighbor = from.IsNeighbor(neighborPoint);
            if (!alreadyNeighbor)
            {
                CPNeighbor newNeighbor = new CPNeighbor();
                newNeighbor.Direction = dir;
                newNeighbor.TargetPoint = neighborPoint;
                newNeighbor.Type = dist < jumpMin ? EnumService.CPNeighborType.ClimbTo : EnumService.CPNeighborType.JumpTo;
                from.m_neighbors.Add(newNeighbor);
                Debug.Log(from.name + " adding neighbor " + neighborPoint.name + " with dist " + dist + " , direction " + dir + ", type " + newNeighbor.Type);
                UnityEditor.EditorUtility.SetDirty(from);
            }
        }
    }
}
