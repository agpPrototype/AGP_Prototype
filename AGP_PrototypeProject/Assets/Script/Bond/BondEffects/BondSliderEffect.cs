using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BondSliderEffect : BondEffect {

	[SerializeField]
	private Slider BondBar;

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
