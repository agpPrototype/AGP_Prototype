using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using UI;
using GameCritical;

namespace Inputs
{
    public class PauseScreenHandler : UIHandler
    {
        [SerializeField]
        private PauseMenu m_PauseMenu;

        [SerializeField]
        private float m_StartButtonDelay = .01f;
        private bool m_CanPressStart;

        void Start()
        {
            m_CanPressStart = true;
            if (m_PauseMenu == null)
                Debug.LogError("Pause Menu is null in PauseScreenHandler");
            else
                m_PauseMenu.gameObject.SetActive(false);
        }

        public override void DoActions(UIActions uia)
        {
            if (uia.Start)
            {
                if(m_CanPressStart) // this is check for timer delay on button press. (will be changed in future)
                {
                    if (m_PauseMenu != null)
                    {
                        StartCoroutine(StartButtonDelay()); // starts coroutine that will deal with button delay for start button press.

                        if (m_PauseMenu.gameObject.activeSelf) // if menu is open then if start is pressed close menu.
                        {
                            m_PauseMenu.gameObject.SetActive(false);

                            if(!UIManager.Instance.TutorialCanvas.TutorialPanel.GetIsActive()) // if tutorial is not open.
                            {
                                GameController.Instance.GameState = EnumService.GameState.InGame;
                            }
                        }
                        else // Show menu if it isn't active.
                        {
                            m_PauseMenu.Show();
                        }
                    }
                }
            }
            if (uia.Back)
            {
                if (m_PauseMenu != null)
                {
                    m_PauseMenu.gameObject.SetActive(false);
                    if (!UIManager.Instance.TutorialCanvas.TutorialPanel.GetIsActive()) // if tutorial is not open.
                    {
                        GameController.Instance.GameState = EnumService.GameState.InGame;
                    }
                }
            }
        }

        private IEnumerator StartButtonDelay()
        {
            m_CanPressStart = false;
            yield return new WaitForSeconds(m_StartButtonDelay);
            m_CanPressStart = true;
        }
    }
}
