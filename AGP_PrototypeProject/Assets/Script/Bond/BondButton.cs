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

	[SerializeField]
	private Canvas MyCanvas;

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
			Vector2 pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(MyCanvas.transform as RectTransform, Input.mousePosition, MyCanvas.worldCamera, out pos);
			transform.position = new Vector3(transform.position.x, MyCanvas.transform.TransformPoint(pos).y, transform.position.z);

			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Clamp(transform.localPosition.y, -75.0f, 75.0f), transform.localPosition.z);

			Bond.BondManager b = Bond.BondManager.Instance;
			if (b)
			{
				b.BondStatus = (int)((transform.localPosition.y + 75.0f) / 1.5f);
			}


			/*
			float mouseY = Input.mousePosition.y;
			float buttonY = this.GetComponent<RectTransform>().position.y;
			

			float v = mouseY - buttonY;

			if (Mathf.Abs(v) > ease)
			{
				Bond.BondManager b = Bond.BondManager.Instance;
				if (b)
				{
					b.BondStatus = b.BondStatus + (int)(v * 13.333f);
				}
			}
			*/
		}
	}
}
