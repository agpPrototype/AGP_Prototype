using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Climb {
    [System.Serializable] 
    public class ClimbPoint : MonoBehaviour {
        public List<CPNeighbor> m_neighbors;
        public List<CPNeighbor> Neighbors
        {
            get { return m_neighbors; }
        }

        // Use this for initialization
        void Start() {
            m_neighbors = new List<CPNeighbor>();
        }

        public void AddNeighbor(ClimbPoint neighborPoint, Vector3 dir, float dist, float jumpMin)
        {
            CPNeighbor newNeighbor = new CPNeighbor();
            newNeighbor.Direction = dir;
            newNeighbor.TargetPoint = neighborPoint;
            newNeighbor.Type = dist < jumpMin ? EnumService.CPNeighborType.ClimbTo : EnumService.CPNeighborType.JumpTo;
            m_neighbors.Add(newNeighbor);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        public void ClearNeighbors()
        {
            m_neighbors.Clear();
        }

        public bool IsNeighbor(ClimbPoint checkPoint)
        {
            for (int i=0; i < m_neighbors.Count; i++)
            {
                if (m_neighbors[i].TargetPoint.transform.position == checkPoint.transform.position)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
