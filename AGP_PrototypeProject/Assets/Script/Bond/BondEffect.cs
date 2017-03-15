using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Bond.BondManager inst = Bond.BondManager.Instance;
		if(inst)
		{
			inst.RegisterEffect(this);
			DoEffect();
		}
	}

	void OnDestroy()
	{
		Bond.BondManager inst = Bond.BondManager.Instance;
		if (inst)
		{
			inst.UnRegisterEffect(this);
		}
	}

	public virtual void DoEffect()
	{
		//WILL OVERRIDE IN CHILD EFFECTS
	}
}
