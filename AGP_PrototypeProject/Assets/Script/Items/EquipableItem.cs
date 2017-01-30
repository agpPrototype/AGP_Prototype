using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public abstract class EquipableItem : MonoBehaviour {

        private Mesh m_mesh;

        public Mesh Mesh
        {
            get
            {
                return m_mesh;
            }
            set
            {
                m_mesh = value;
            }
        }

        // Use this for initialization
        void Start () 
        {

        }

        // Update is called once per frame
        void Update () 
        {

        }

        /// <summary>
        /// Virtual function for doing an Action
        /// </summary>
        public virtual void DoAction()
        {

        }

        /// <summary>
        /// Virtual function for when button is pressed to do action
        /// </summary>
        public virtual void DoActionBegin()
        {

        }

        /// <summary>
        /// Virtual function for when button released to do action
        /// </summary>
        public virtual void DoActionRelease()
        {

        }

    }
}