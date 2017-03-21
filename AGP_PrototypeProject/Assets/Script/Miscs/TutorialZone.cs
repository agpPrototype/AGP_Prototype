using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;
using AI;
using Player;

namespace Misc
{
    public class TutorialZone : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Category of the tutorial tip. Shown at top of tutorial panel.")]
        private string m_Title;

        [SerializeField]
        [Tooltip("Text to be shown as tutorial tip.")]
        private string m_Tip;

        [SerializeField]
        private RectTransform m_UIPrefabToSpawn;

        [SerializeField]
        [Tooltip("If true then when the player enters this tutorial zone then a tip will show.")]
        private bool m_ShowTip = false;

        [SerializeField]
        [Tooltip("If true then player can't move when entering.")]
        private bool m_FreezePlayer = true;


        private PlayerControl m_PlayerControl;

        void Start()
        {
            m_PlayerControl = FindObjectOfType<PlayerControl>();
        }

        public virtual void OnTutorialZoneEnter(Collider col)
        {
            if(m_ShowTip)
            {
                TutorialPanel tutorialPanel = UIManager.Instance.TutorialCanvas.TutorialPanel;
                if (tutorialPanel != null)
                {
                    tutorialPanel.PopulatePanel(m_Title, m_Tip, m_UIPrefabToSpawn);
                    tutorialPanel.SlideIn();
                }
            }

            if(m_FreezePlayer)
            {
                if(m_PlayerControl != null)
                {
                    m_PlayerControl.enabled = false;
                }
            }
        }

        public virtual void OnTutorialZoneExit(Collider col)
        {
            TutorialPanel tutorialPanel = UIManager.Instance.TutorialCanvas.TutorialPanel;
            if (tutorialPanel != null)
            {
                tutorialPanel.SlideOut();
            }

            if (m_FreezePlayer)
            {
                if (m_PlayerControl != null)
                {
                    m_PlayerControl.enabled = true;
                }
            }
        }

        public virtual void OnTutorialZoneStay(Collider col) { }

        void OnTriggerEnter(Collider col)
        {
            // check to see if a player entered this trigger.
            if (col.GetComponent<Player.PlayerControl>() == null)
            {
                return;
            }

            OnTutorialZoneEnter(col);
        }

        void OnTriggerExit(Collider col)
        {
            // check to see if a player exited this trigger.
            if (col.GetComponent<Player.PlayerControl>() == null)
            {
                return;
            }

            OnTutorialZoneExit(col);
        }

        void OnTriggerStay(Collider col)
        {
            // check to see if a player staying in this trigger.
            if (col.GetComponent<Player.PlayerControl>() == null)
            {
                return;
            }

            OnTutorialZoneStay(col);
        }
    }
}
