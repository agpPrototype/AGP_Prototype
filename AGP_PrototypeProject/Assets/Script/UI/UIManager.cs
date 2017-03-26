using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inputs;

/*
 * author: rob neir
 * date: 1/20/2017 
 * 
 * */
namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public TutorialCanvas TutorialCanvas;
        public EndScreenCanvas EndScreenCanvas;
        public PauseScreenCanvas PauseScreenCanvas;
        public HUDCanvas HUDCanvas;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        void Start()
        {
            if (TutorialCanvas == null)
            {
                TutorialCanvas = FindObjectOfType<TutorialCanvas>();
            }
        }
    }
}
