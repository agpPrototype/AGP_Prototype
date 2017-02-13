using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class PatrolArea : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("How high up the patrol icon is.")]
        private float PatrolIconHeight;

        [SerializeField]
        [Tooltip("Boundary color of debug lines drawn for patrol area")]
        private Color PatrolBoundaryColor;

        [SerializeField]
        [Tooltip("Waypoint prefab used when adding waypoints.")]
        private Waypoint WaypointPrefab;

        private List<Waypoint> m_Waypoints; // List of waypoints that define patrol area.

        // Use this for initialization
        void Start()
        {
            // For each child add the child to the list of waypoints if they were not added.
            m_Waypoints = new List<Waypoint>();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                Waypoint waypointComponent = child.GetComponent<Waypoint>();
                if (waypointComponent != null)
                {
                    if(!m_Waypoints.Contains(waypointComponent))
                    {
                        m_Waypoints.Add(waypointComponent);
                    }
                }
            }
        }

        /* Gets the next waypoint in the list of waypoints.
         This is usually used for predicatble AI that follow a path.*/
        public Waypoint GetNextWaypoint(Waypoint currWaypoint)
        {
            // If we don't have more than 1 waypoint then return the only waypoint in the patrol area.
            if(currWaypoint == null)
            {
                return (m_Waypoints.Count > 0) ? m_Waypoints[0] : null;
            }
            int indexOfCurrWaypoint = m_Waypoints.IndexOf(currWaypoint);
            return m_Waypoints[(indexOfCurrWaypoint+1) % (m_Waypoints.Count)];
        }

        /* Gets the random waypoint from the list of waypoints.
         This is usually used for unpredicatble AI that do not follow a path.*/
        public Waypoint GetRandomWaypoint()
        {
            if(m_Waypoints.Count == 0)
            {
                return null;
            }
            return m_Waypoints[Random.Range(0, m_Waypoints.Count)];
        }
        
        /* Called to spawn a new waypoint that is added to this patrol area. */
        public void InstantiateWaypoint()
        {
            if(WaypointPrefab == null)
            {
                Debug.Log("No waypoint prefab specified for patrol area script.");
                return;
            }
            Waypoint WaypointInstance = Instantiate(WaypointPrefab, this.transform, false);
            WaypointInstance.transform.localPosition = new Vector3(0, 0, -2);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnDrawGizmos()
        {
            // Draw patrol area icon
            Gizmos.DrawIcon(this.transform.position + Vector3.up * PatrolIconHeight, "patrollingIcon.png");
            if(m_Waypoints != null)
            {
                for (int i = 0; i < m_Waypoints.Count; i++)
                {
                    Waypoint currWaypoint = m_Waypoints[i];
                    Waypoint nextWaypoint = m_Waypoints[(i + 1) % m_Waypoints.Count];
                    Gizmos.color = PatrolBoundaryColor;
                    Gizmos.DrawLine(currWaypoint.transform.position, nextWaypoint.transform.position);
                }
            }
        }
    }
}
