using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Inputs;

/*
 * author: rob neir
 * date: 3/19/2017 
 * 
 * */

[RequireComponent (typeof(Animator))]
public class TutorialPanel : MonoBehaviour {

    [SerializeField]
    private Text m_TitleText;

    [SerializeField]
    private Text m_TipText;

    [SerializeField]
    private Transform m_UIPrefabSpawnPoint;

    private Animator m_Animator;

    // Use this for initialization
    void Start () {
        m_Animator = GetComponent<Animator>();

        if (m_TipText == null)
            Debug.LogError("Tip text is null in TutorialPanel");
        if (m_TitleText == null)
            Debug.LogError("Title text is null in TutorialPanel");
        if (m_UIPrefabSpawnPoint == null)
            Debug.LogError("UI prefab spawn point is null in TutorialPanel");
    }

    public void SlideIn()
    {
        m_Animator.SetBool("IsOnScreen", true);
    }

    public void SlideOut()
    {
        m_Animator.SetBool("IsOnScreen", false);
    }

    public void PopulatePanel(string title, string tip, RectTransform prefabToSpawn)
    {
        m_TitleText.text = title;
        m_TipText.text = tip;

        // Destroy all children of the prefab spawn point.
        int numChilds = m_UIPrefabSpawnPoint.childCount;
        for(int i=0; i < numChilds; i++)
        {
            GameObject childGO = m_UIPrefabSpawnPoint.GetChild(i).gameObject;
            if(childGO != null)
            {
                Destroy(childGO);
            }
        }

        // spawn new prefab on prefab spawn point if it was specified.
        if(prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, m_UIPrefabSpawnPoint, false);
        }
    }
}
