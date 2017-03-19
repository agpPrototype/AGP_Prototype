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
    private Image m_Image;

    private Animator m_Animator;

    // Use this for initialization
    void Start () {
        m_Animator = GetComponent<Animator>();

        if (m_TipText == null)
            Debug.LogError("Tip text is null in TutorialPanel");
        if (m_TitleText == null)
            Debug.LogError("Title text is null in TutorialPanel");
        if (m_Image == null)
            Debug.LogError("Image is null in TutorialPanel");
    }

    public void SlideIn()
    {
        m_Animator.SetBool("IsOnScreen", true);
    }

    public void SlideOut()
    {
        m_Animator.SetBool("IsOnScreen", false);
    }

    public void PopulatePanel(string title, string tip, Sprite sprite)
    {
        m_TitleText.text = title;
        m_TipText.text = tip;
        m_Image.sprite = sprite;
        Debug.Log(tip);
    }
}
