using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI;

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
        [Tooltip("Sprite to be shown on tutorial panel.")]
        private Sprite m_Sprite;

        [SerializeField]
        [Tooltip("If true will closes the tutorial window for the player when exiting this zone. The player otherwise can close by themselves when they want.")]
        private bool m_CloseOnExit;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter(Collider col)
        {
            // check to see if a player entered this trigger.
            if (col.GetComponent<Player.PlayerControl>() == null)
            {
                return;
            }

            TutorialCanvas tutorialCanvas = UIManager.Instance.TutorialCanvas;
            if (tutorialCanvas == null)
            {
                return;
            }

            TutorialPanel tutorialPanel = tutorialCanvas.TutorialPanel;
            if (tutorialPanel == null)
            {
                return;
            }

            tutorialPanel.PopulatePanel(this.m_Title, this.m_Tip, this.m_Sprite);
            tutorialPanel.SlideIn();
        }

        void OnTriggerExit(Collider col)
        {
            if(!m_CloseOnExit)
            {
                return;
            }

            // check to see if a player exited this trigger.
            if (col.GetComponent<Player.PlayerControl>() == null)
            {
                return;
            }

            TutorialCanvas tutorialCanvas = UIManager.Instance.TutorialCanvas;
            if(tutorialCanvas == null)
            {
                return;
            }

            TutorialPanel tutorialPanel = tutorialCanvas.TutorialPanel;
            if (tutorialPanel == null)
            {
                return;
            }

            tutorialPanel.SlideOut();
        }
    }
}
