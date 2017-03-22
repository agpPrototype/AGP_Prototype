using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BondButton : MonoBehaviour {

	[SerializeField]
	private Slider BondSlider;

	[Tooltip("How easy the slider will drag.")]
	[SerializeField]
	private float ease = 0.0f;

	private bool m_CanDrag = false;

	public void UpdatePosition()
	{
		float newY = 1.5f * (BondSlider.value - 50.0f);
		this.gameObject.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, newY, this.gameObject.transform.localPosition.z);
	}

	public void CanDrag()
	{
		m_CanDrag = true;
	}

	public void StopDrag()
	{
		m_CanDrag = false;
	}

	void Update()
	{
		
		if (m_CanDrag && Input.GetMouseButton(0))
		{
			float mouseY = Input.mousePosition.y;
			float buttonY = this.gameObject.transform.position.y;

			float v = mouseY - buttonY;

			if (Mathf.Abs(v) > ease)
			{
				Bond.BondManager b = Bond.BondManager.Instance;
				if (b)
				{
					b.BondStatus = b.BondStatus + (int)(v * 0.75f);
				}
			}
		}
	}
}
