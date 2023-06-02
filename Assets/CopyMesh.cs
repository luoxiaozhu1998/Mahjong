using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Avatar2;
using UnityEngine;
using UnityEngine.Rendering;

public class CopyMesh : MonoBehaviour
{
    [SerializeField] private float rayDistance = 2.0f;
    [SerializeField] private float rayWidth = 0.01f;

    [SerializeField] private Color rayColorDefault = Color.yellow;
    [SerializeField] private Color rayColorHover = Color.red;
    public Transform GazePos;
    private LineRenderer _lineRenderer;
    public GameObject effectPrefab;

    private void Awake()
    {
        // _lineRenderer = GetComponent<LineRenderer>();
        // SetupRay();
    }

    public void SetupRay()
    {
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = rayWidth;
        _lineRenderer.endWidth = rayWidth;
        _lineRenderer.startColor = _lineRenderer.endColor = rayColorDefault;
    }

    public static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
    {
        Mesh meshCopy = new Mesh();
        meshCopy.indexFormat = nonReadableMesh.indexFormat;

        // Handle vertices
        GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
        int totalSize = verticesBuffer.stride * verticesBuffer.count;
        byte[] data = new byte[totalSize];
        verticesBuffer.GetData(data);
        meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
        meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
        verticesBuffer.Release();

        // Handle triangles
        meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
        GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
        int tot = indexesBuffer.stride * indexesBuffer.count;
        byte[] indexesData = new byte[tot];
        indexesBuffer.GetData(indexesData);
        meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
        meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
        indexesBuffer.Release();

        // Restore submesh structure
        uint currentIndexOffset = 0;
        for (int i = 0; i < meshCopy.subMeshCount; i++)
        {
            uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
            meshCopy.SetSubMesh(i, new SubMeshDescriptor((int) currentIndexOffset, (int) subMeshIndexCount));
            currentIndexOffset += subMeshIndexCount;
        }

        // Recalculate normals and bounds
        meshCopy.RecalculateNormals();
        meshCopy.RecalculateBounds();
        Debug.Log(meshCopy.vertices.Length);
        return meshCopy;
    }

    private void Update()
    {
        GenerateEffect();
    }

    public void GenerateEffect()
    {
        Instantiate(effectPrefab, transform.position, transform.rotation);
    }
}