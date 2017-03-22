using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace UI
{
    public class PauseMenu : Menu
    {

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