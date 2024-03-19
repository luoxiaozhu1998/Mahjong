using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TestScript : MonoBehaviour
{
	[FormerlySerializedAs("_count")] public int count = 1;

	[FormerlySerializedAs("_isTiming")] public bool isTiming = false;

	[FormerlySerializedAs("_startTime")] public float startTime;
	[FormerlySerializedAs("_endTime")] public float endTime;

	[FormerlySerializedAs("_totalTime")] public float totalTime;

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void AddCount()
	{
		count++;
		Debug.Log(count);
		if (isTiming)
		{
			endTime = Time.time;
			totalTime += endTime - startTime;
		}

		if (count > 4)
		{
			Debug.Log(totalTime / count);
			count = 1;
			totalTime = 0;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Hand"))
		{
			Debug.Log("开始计时");
			isTiming = true;
			startTime = Time.time;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Hand"))
		{
			Debug.Log("结束计时");
			isTiming = false;
		}
	}
}