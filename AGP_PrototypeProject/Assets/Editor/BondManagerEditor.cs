using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bond
{
	[CustomEditor(typeof(BondManager))]
	public class BondManagerEditor : Editor
	{

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			BondManager manager = (BondManager)target;
			if(GUILayout.Button("Update Slider"))
			{
				manager.UpdateEffects();
			}
		}
	}
}