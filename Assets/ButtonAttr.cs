using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonAttr : MonoBehaviour
{
	private BoxCollider _boxCollider;
	public CopyMesh CopyMesh;
	public ButtonAttr otherButton;
	private bool isCountMove = true;
	public bool isCountSelect = false;
	public bool eyeEnter = false;
	private float enterTime;

	private void Awake()
	{
		_boxCollider = GetComponent<BoxCollider>();
	}

	public void OnEyeEnter()
	{
		//大型按钮
		// CopyMesh.AddCount();
		// gameObject.SetActive(false);
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	1040f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	195f);
		// _boxCollider.size = new Vector3(1040f, 195f, 2f);

		//中型按钮
		//CopyMesh.AddCount();
		//gameObject.SetActive(false);
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	715f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	195f);
		// _boxCollider.size = new Vector3(715f, 195f, 2f);

		//小型按钮
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	220f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	110f);
		// _boxCollider.size = new Vector3(220f, 110f, 2f);
		if (isCountMove)
		{
			CopyMesh.StartTimer();
			//enterTime = Time.time;
			isCountMove = false;
		}
		// if (eyeEnter) return;
		// eyeEnter = true;
		// enterTime = Time.time;
	}


	public void OnEyeExit()
	{
		//大型按钮
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	800f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	150f);
		// _boxCollider.size = new Vector3(800f, 150f, 2f);

		//中型按钮
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	550f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	150f);
		// _boxCollider.size = new Vector3(550f, 150f, 2f);
		//小型按钮
		// var trans = transform as RectTransform;
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
		// 	200f);
		// trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
		// 	100f);
		// _boxCollider.size = new Vector3(200f, 100f, 2f);
	}

	public void OnClick()
	{
		otherButton.isCountMove = true;
	}

	public void AddSelect()
	{
		CopyMesh.AddSelect(Time.time - enterTime);
	}
}