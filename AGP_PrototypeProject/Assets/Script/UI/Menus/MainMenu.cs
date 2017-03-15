using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    public class MainMenu : Menu
    {

        public string m_StartingLevel = "";

        public void NewGameClicked()
        {
            SceneManager.LoadScene(m_StartingLevel);
        }

        public void SettingsClicked()
        {
            Debug.Log("Settings Button Clicked");
        }

        public void LoadGameClicked()
        {
            SceneManager.LoadScene(m_StartingLevel);
        }

        public void QuitClicked()
        {
            Application.Quit();
        }
    } // MainMenu class
} // UI namespace
