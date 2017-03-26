using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Inputs;

/// <summary>
/// author: Rob Neir
/// </summary>
/// 

namespace UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioContainer))]
    public class TutorialPanel : UIComponent
    {
        [SerializeField]
        private Text m_TitleText;

        [SerializeField]
        private Text m_TipText;

        [SerializeField]
        private Transform m_UIPrefabSpawnPoint;

        private Animator m_Animator;
        private AudioContainer m_AudioContainer;
        public AudioContainer AudioContainer { get { return m_AudioContainer; } }

        // Use this for initialization
        void Start()
        {
            m_Animator = GetComponent<Animator>();
            m_AudioContainer = GetComponent<AudioContainer>();

            if (m_TipText == null)
                Debug.LogError("Tip text is null in TutorialPanel");
            if (m_TitleText == null)
                Debug.LogError("Title text is null in TutorialPanel");
            if (m_UIPrefabSpawnPoint == null)
                Debug.LogError("UI prefab spawn point is null in TutorialPanel");
        }

        public void SlideIn()
        {
            m_AudioContainer.Play2DSound(1);
            m_Animator.SetBool("IsOnScreen", true);
            Activate();
        }

        public void SlideOut()
        {
            m_Animator.SetBool("IsOnScreen", false);
            Deactivate();
        }

        public void PopulatePanel(string title, string tip, RectTransform prefabToSpawn)
        {
            m_TitleText.text = title;
            m_TipText.text = tip;

            // Destroy all children of the prefab spawn point.
            int numChilds = m_UIPrefabSpawnPoint.childCount;
            for (int i = 0; i < numChilds; i++)
            {
                GameObject childGO = m_UIPrefabSpawnPoint.GetChild(i).gameObject;
                if (childGO != null)
                {
                    Destroy(childGO);
                }
            }

            // spawn new prefab on prefab spawn point if it was specified.
            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, m_UIPrefabSpawnPoint, false);
            }
        }
    }
}
