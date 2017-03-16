using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using AI;

namespace Player
{
    public class CommandHandler : MonoBehaviour
    {
        private CompanionAISM m_CompanionAISM;
        private float m_MoveToDelay;
        private float m_StayDelay;

        public void SetCompanionAISM(CompanionAISM wolfAI)
        {
            m_CompanionAISM = wolfAI;
        }
        public void ProcessCommands(PCActions pca)
        {
            if (pca.InputPackets[(int)EnumService.InputType.LT] != null)
            {
                pca.Aim = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.LT].Value);
            }

            if (pca.InputPackets[(int)EnumService.InputType.RB] != null)
            {
                pca.MoveTo = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.RB].Value);
            }

            if (pca.InputPackets[(int)EnumService.InputType.LB] != null)
            {
                pca.Stay = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.LB].Value);
            }

            DoCommands(pca);
        }

        private void DoCommands(PCActions pca)
        {
            if (pca.Aim && pca.MoveTo && m_MoveToDelay > 0.5f)
            {
                m_MoveToDelay = 0.0f;
                //ray hit point
                Vector3 rayHitPoint = Vector3.zero;
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
                Vector3 worldSpace = Vector3.zero;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    worldSpace = hit.point;
                    rayHitPoint = worldSpace;
                    //m_CompanionAISM.MoveTo(worldSpace, hit.transform.gameObject);
                }
                else
                {
                    Debug.Log("WARNING: invalid location, can't issue moveto command");
                }
            }

            if (pca.Stay && m_StayDelay > 0.5f)
            {
                m_StayDelay = 0.0f;
                //m_CompanionAISM.Stay();
            }
        }

        void Update()
        {
            m_MoveToDelay += Time.deltaTime;
            m_StayDelay += Time.deltaTime;
            if (m_MoveToDelay > 10)
            {
                m_MoveToDelay = 0.0f;
            }
            if (m_StayDelay > 10)
            {
                m_StayDelay = 0.0f;
            }
        }

    }
}
