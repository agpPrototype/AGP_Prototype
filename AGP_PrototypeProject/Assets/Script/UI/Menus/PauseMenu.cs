﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameCritical;
using Utility;


/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    public class PauseMenu : Menu
    {
        [SerializeField]
        [Tooltip("Text that changes depending on what menu we are showing in pause menu.")]
        private Text m_TitleText;

        // shows the pause menu.
        public override void Show()
        {
            gameObject.SetActive(true);
            UpdateTitleText();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(m_StartingSelectedButton.gameObject);
            GameController.Instance.GameState = EnumService.GameState.InPauseMenu;
        }

        // update the title text which matches the current pause menu screen.
        public void UpdateTitleText()
        {
            GameObject selectedGO = EventSystem.current.currentSelectedGameObject;
            if(selectedGO != null)
            {
                Text txt = selectedGO.GetComponentInChildren<Text>();
                if (txt != null && m_TitleText != null)
                {
                    m_TitleText.text = txt.text;
                }
            }
        }

        public void RestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SaveClicked()
        {
            Debug.Log("Save Button Clicked");
        }

        public void LoadClicked()
        {
            Debug.Log("Load Button Clicked");
        }

        public void SettingsClicked()
        {
            Debug.Log("Settings Button Clicked");
        }

        public void ControlsClicked()
        {
            Debug.Log("Controls Button Clicked");
        }

        public void QuitClicked()
        {
            Application.Quit();
        }
    } // MainMenu class
} // UI namespace