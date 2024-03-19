using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetProjectionMatrix : MonoBehaviour
{
	public float hor;

	// Start is called before the first frame update
	void Start()
	{
		var mat = GetComponent<Camera>().projectionMatrix;
		mat[0, 2] = hor;
		GetComponent<Camera>().projectionMatrix = mat;
	}

	// Update is called once per frame
	void Update()
	{
	}
}