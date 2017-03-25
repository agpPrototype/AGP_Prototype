using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EndGame {
public class PeaceTreeParentHandler : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		foreach (PeaceTreeHandler child in GetComponentsInChildren(typeof(PeaceTreeHandler), true))
        {
            child.gameObject.SetActive(true);
        }
	}
	
}
}
