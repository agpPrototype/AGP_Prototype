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
        private float m_ComeDelay;

        [SerializeField]
        private float m_MaxDistanceMoveTo;

        void Start()
        {
            //m_CompanionAISM = GameCritical.GameController.Instance.Wolf.GetComponent<CompanionAISM>();
        }

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

            if (pca.InputPackets[(int)EnumService.InputType.DDown] != null)
            {
                pca.Stay = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.DDown].Value);
            }

            if (pca.InputPackets[(int)EnumService.InputType.Square] != null)
            {
                pca.Come = Convert.ToBoolean(pca.InputPackets[(int)EnumService.InputType.Square].Value);
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
                    // Bypass the collider of 1 Action Zone so that you can call "MoveTo" from outside of an Action Zone
                    if (hit.collider.gameObject.GetComponent<ActionZone>())
                    {
                        ray = new Ray(hit.point, ray.direction);
                        if (Physics.Raycast(ray, out hit))
                        {
                            if (hit.collider.gameObject.GetComponent<ActionZone>())
                            {
                                return;
                            }
                        }
                    }

                    if (!hit.collider || hit.point == Vector3.zero)
                        return;

                    worldSpace = hit.point;
                    rayHitPoint = worldSpace;

                    // Limit command to a certain range
                    float distSq = (rayHitPoint - m_CompanionAISM.gameObject.transform.position).sqrMagnitude;
                    if(distSq < m_MaxDistanceMoveTo * m_MaxDistanceMoveTo)
                        m_CompanionAISM.GiveGoToCommand(hit.transform.gameObject, worldSpace);
                    else
                    {
                        Debug.Log("Cannot command Accalia to move that far! Distance Given: " + Mathf.Sqrt(distSq) + ", Max is:" + m_MaxDistanceMoveTo);
                    }
                }
                else
                {
                    Debug.Log("WARNING: invalid location, can't issue moveto command");
                }
            }

            if (pca.Stay && m_StayDelay > 0.5f)
            {
                Debug.Log("Gave 'Stay' command");
                m_StayDelay = 0.0f;
                m_CompanionAISM.GiveCommand(WolfCommand.STAY);
            }

            if (pca.Come && m_ComeDelay > 0.5f)
            {
                Debug.Log("Gave 'Come' command");
                m_ComeDelay = 0.0f;
                m_CompanionAISM.GiveCommand(WolfCommand.COME);
            }
        }

        void Update()
        {
            m_MoveToDelay += Time.deltaTime;
            m_StayDelay += Time.deltaTime;
            m_ComeDelay += Time.deltaTime;
            if (m_MoveToDelay > 10)
            {
                m_MoveToDelay = 0.0f;
            }
            if (m_StayDelay > 10)
            {
                m_StayDelay = 0.0f;
            }
            if (m_ComeDelay > 10)
            {
                m_ComeDelay = 0.0f;
            }
        }

    }
}
