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

        // Use this for initialization
        void Start()
        {
            m_Location = gameObject.transform.position;
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