using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondWolfColorEffect : BondEffect {

	[SerializeField]
	private SkinnedMeshRenderer Magic;

	[SerializeField]
	private Color LowBondColor;

	[SerializeField]
	private Color HighBondColor;

	public override void DoEffect()
	{
		if(!Magic)
		{
			Debug.LogError("Magic missing from scene");
			return;
		}
		float bond = Bond.BondManager.Instance.GetBondStatus01();

		Color c = Color.Lerp(LowBondColor, HighBondColor, bond);
		Magic.material.color = c;
	}
}
