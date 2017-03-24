using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BondSliderEffect : BondEffect {

	[SerializeField]
	private Slider BondBar;

    [SerializeField]
    private Image m_FadeImage;

    [SerializeField]
    private Color m_GoodBondFadeColor;

    [SerializeField]
    private Color m_BadBondFadeColor;

    [SerializeField]
    private Color m_NeturalBondFadeColor;

    public void ShowBadBondFade()
    {
        m_FadeImage.color = m_GoodBondFadeColor;
    }

    public void ShowGoodBondFade()
    {
        m_FadeImage.color = m_BadBondFadeColor;
    }

    public void ShowNeutralBondFade()
    {
        m_FadeImage.color = m_NeturalBondFadeColor;
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
