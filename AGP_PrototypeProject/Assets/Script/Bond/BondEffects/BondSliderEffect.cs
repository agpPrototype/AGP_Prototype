using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class BondSliderEffect : BondEffect {

	[SerializeField]
	private Slider BondBar;

    [SerializeField]
    private ImageGlowEffect m_BondBarFade;
    [SerializeField]
    private Color m_GoodBondFadeColor;
    [SerializeField]
    private Color m_BadBondFadeColor;
    [SerializeField]
    private Color m_NeturalBondFadeColor;

    [SerializeField]
    private Text m_Text;

    public enum BondLevel
    {
        Good,
        Bad,
        Neutral
    }

    void Start()
    {
        SetBondLevel(BondLevel.Neutral, "Bond - neutral");
    }

    public void SetBondLevel(BondLevel level, string text)
    {
        switch (level)
        {
            case BondLevel.Good:
                m_BondBarFade.m_TransitionSpeed = 4f;
                m_BondBarFade.SetStartColor(m_NeturalBondFadeColor);
                m_BondBarFade.SetEndColor(m_GoodBondFadeColor);
                m_Text.text = text;
                break;
            case BondLevel.Bad:
                m_BondBarFade.m_TransitionSpeed = 4f;
                m_BondBarFade.SetStartColor(m_NeturalBondFadeColor);
                m_BondBarFade.SetEndColor(m_BadBondFadeColor);
                m_Text.text = text;
                break;
            case BondLevel.Neutral:
                m_BondBarFade.m_TransitionSpeed = 4f;
                m_BondBarFade.SetStartColor(m_NeturalBondFadeColor);
                m_BondBarFade.SetEndColor(m_NeturalBondFadeColor);
                m_Text.text = text;
                break;
        }
    }

	public override void DoEffect() 
	{
		if (!BondBar)
		{
			Debug.LogError("BondBar missing from scene");
			return;
		}

		int newBond = Bond.BondManager.Instance.BondStatus;

		BondBar.GetComponent<Slider>().value = newBond;
	}
}
