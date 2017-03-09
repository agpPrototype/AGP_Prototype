using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Utility;
using System;

namespace Items
{
    public class EquipmentHandler : MonoBehaviour
    {
        Animator m_Animator;
        [System.Serializable]
        public class PlayerSettings
        {
            public Transform RightHand;
            public Transform LeftHand;
            public Transform BowUneuipedPos;
        }

        [SerializeField]
        private PlayerSettings m_PlayerSettings;
        
        [System.Serializable]
        public class Animations
        {

        }
        [SerializeField]
        private Animations m_Animations;    

        [SerializeField]
        private WeaponBow Bow;

        public List<EquipableItem> ItemList;

        private PlayerControl m_PlayerControl;
        private MoveComponent m_MoveComp;
        private bool m_Aim;
        


        // Use this for initialization
        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_PlayerControl = GetComponent<PlayerControl>();
            m_MoveComp = GetComponent<MoveComponent>();
        }

        // Update is called once per frame
        public void ProcessActions(PCActions pca)
        {
            if (pca.InputPackets[(int)EnumService.InputType.LT] != null)
            {
                pca.Aim = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.LT].Value);
            }

            if (pca.InputPackets[(int)EnumService.InputType.RT] != null)
            {
                pca.Fire = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.RT].Value);
            }


            DoActions(pca);
        }


        private void DoActions(PCActions pca)
        {
            if (pca.Aim && pca.Fire)
            {
                //FIRE
                if (Bow)
                {
                    Bow.DoAction();
                }
            }
        }
    }
}
