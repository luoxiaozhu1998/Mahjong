using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CopyMesh : MonoBehaviour
{
	// [SerializeField] private float rayDistance = 2.0f;
	// [SerializeField] private float rayWidth = 0.01f;
	//
	// [SerializeField] private Color rayColorDefault = Color.yellow;
	// [SerializeField] private Color rayColorHover = Color.red;
	// public Transform GazePos;
	// private LineRenderer _lineRenderer;
	// public GameObject effectPrefab;
	//
	// private void Awake()
	// {
	//     // _lineRenderer = GetComponent<LineRenderer>();
	//     // SetupRay();
	// }
	//
	// public void SetupRay()
	// {
	//     _lineRenderer.positionCount = 2;
	//     _lineRenderer.startWidth = rayWidth;
	//     _lineRenderer.endWidth = rayWidth;
	//     _lineRenderer.startColor = _lineRenderer.endColor = rayColorDefault;
	// }
	//
	// public static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
	// {
	//     Mesh meshCopy = new Mesh();
	//     meshCopy.indexFormat = nonReadableMesh.indexFormat;
	//
	//     // Handle vertices
	//     GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
	//     int totalSize = verticesBuffer.stride * verticesBuffer.count;
	//     byte[] data = new byte[totalSize];
	//     verticesBuffer.GetData(data);
	//     meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
	//     meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
	//     verticesBuffer.Release();
	//
	//     // Handle triangles
	//     meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
	//     GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
	//     int tot = indexesBuffer.stride * indexesBuffer.count;
	//     byte[] indexesData = new byte[tot];
	//     indexesBuffer.GetData(indexesData);
	//     meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
	//     meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
	//     indexesBuffer.Release();
	//
	//     // Restore submesh structure
	//     uint currentIndexOffset = 0;
	//     for (int i = 0; i < meshCopy.subMeshCount; i++)
	//     {
	//         uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
	//         meshCopy.SetSubMesh(i, new SubMeshDescriptor((int) currentIndexOffset, (int) subMeshIndexCount));
	//         currentIndexOffset += subMeshIndexCount;
	//     }
	//
	//     // Recalculate normals and bounds
	//     meshCopy.RecalculateNormals();
	//     meshCopy.RecalculateBounds();
	//     Debug.Log(meshCopy.vertices.Length);
	//     return meshCopy;
	// }
	//
	// // private void Update()
	// // {
	// //     GenerateEffect();
	// // }
	//
	// public void Test()
	// {
	//     Debug.Log("hover");
	// }
	//
	// public void Test2()
	// {
	//     Debug.Log("unhover");
	// }
	//
	// public void GenerateEffect()
	// {
	//     Instantiate(effectPrefab, transform.position, transform.rotation);
	// }
	[SerializeField] private GazeInteractor gazeInteractor;
	[SerializeField] private GameObject leftRayInteractor;
	[SerializeField] private GameObject rightRayInteractor;
	[SerializeField] private Transform gazeIcon;
	public Transform LeftEyeAnchor;
	public Transform RightEyeAnchor;
	public Transform CenterEyeAnchor;
	private float startTime;
	private float totalTime;
	private int count;
	public ButtonAttr ButtomButton;
	public ButtonAttr TopButton;
	private float moveTime = 0f;
	private float selectTime = 0f;
	private int pinchCount = 0;

	public void EnableEyeGaze()
	{
		if (!gazeInteractor.enabled)
		{
			gazeInteractor.enabled = true;
			rightRayInteractor.gameObject.SetActive(false);
			leftRayInteractor.gameObject.SetActive(false);
			//startTime = Time.time;
			//ButtomButton.isCountMove = true;
		}
		// else
		// {
		// 	Debug.Log($"移动时间:{Time.time - startTime}");
		// }
	}

	public TMP_Text message;

	public void AddCount()
	{
		count++;
		//message.text = count.ToString();
		totalTime += Time.time - startTime;
		if (count == 8)
		{
			Debug.Log($"总时间：{totalTime}");
			Debug.Log($"正确率：{8.0f / pinchCount}");
			//Debug.Log($"移动时间：{moveTime}");
		}
	}

	public void DisableEyeGaze()
	{
		if (gazeInteractor.enabled)
		{
			gazeInteractor.enabled = false;
			rightRayInteractor.gameObject.SetActive(true);
			leftRayInteractor.gameObject.SetActive(true);
			gazeIcon.position = Vector3.zero;
		}
	}

	public void AddMove(float time)
	{
		moveTime += time;
	}

	public void AddSelect(float time)
	{
		selectTime += time;
	}

	public void StartTimer()
	{
		startTime = Time.time;
	}

	public void AddPinch()
	{
		pinchCount++;
	}
}