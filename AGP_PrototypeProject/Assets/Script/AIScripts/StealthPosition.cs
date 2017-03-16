using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class StealthPosition : MonoBehaviour
    {

        [SerializeField]
        private Vector3 m_Location;

        [SerializeField]
        private StealthPosition[] m_NextPositions;

        [SerializeField]
        private Vector3 m_WolfAlignmentDirection;
        public Vector3 AlignmentDir
        {
            get { return m_WolfAlignmentDirection; }
        }

        [SerializeField]
        public string m_name;

        // Used for Path navigation only
        private bool m_Visited;
        public bool Visited
        {
            get { return m_Visited; }
            set { m_Visited = value; }
        }

        // Used for Path navigation only
        private StealthPosition m_ParentForPath;
        public StealthPosition ParentForPath
        {
            get { return m_ParentForPath; }
            set { m_ParentForPath = value; }
        }

        // Use this for initialization
        void Start()
        {
            m_Location = gameObject.transform.position;
            m_Visited = false;
        }

        // Update is called once per frame
        void Update()
        {
        }

        public StealthPosition[] GetNextPositions()
        {

            return m_NextPositions;
        }


    }

}