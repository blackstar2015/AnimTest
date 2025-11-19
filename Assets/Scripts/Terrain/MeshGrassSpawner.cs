using UnityEngine;
using System.Runtime.InteropServices;

public class MeshGrassSpawner : MonoBehaviour
{
    [Header("Grass Data")]
    public Mesh grassMesh;
    public Material grassMaterial;
    public int instanceCount = 1000000;

    [Header("Bounds")]
    public Vector3 center = Vector3.zero;
    public Vector3 size = new Vector3(500f, 50f, 500f);

    ComputeBuffer positionBuffer;
    ComputeBuffer rotationBuffer;
    ComputeBuffer scaleBuffer;
    ComputeBuffer argsBuffer;

    Material runtimeMaterial;

    void Start()
    {
        // clone material so we can modify it safely
        runtimeMaterial = new Material(grassMaterial);
        runtimeMaterial.SetInt("_VertexCountPerInstance", grassMesh.vertexCount); 
        runtimeMaterial.enableInstancing = true;
        CreateBuffers();
    }

    void CreateBuffers()
    {
        // ------------ INSTANCE DATA BUFFERS ----------------
        Vector3[] positions = new Vector3[instanceCount];
        Vector3[] rotations = new Vector3[instanceCount];
        Vector3[] scales = new Vector3[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            positions[i] = new Vector3(
                Random.Range(center.x - size.x * 0.5f, center.x + size.x * 0.5f),
                center.y,
                Random.Range(center.z - size.z * 0.5f, center.z + size.z * 0.5f)
            );

            rotations[i] = new Vector3(0, Random.Range(0, 360), 0);
            scales[i] = Vector3.one;
        }

        int stride = Marshal.SizeOf(typeof(Vector3));

        positionBuffer = new ComputeBuffer(instanceCount, stride, ComputeBufferType.Structured);
        rotationBuffer = new ComputeBuffer(instanceCount, stride, ComputeBufferType.Structured);
        scaleBuffer = new ComputeBuffer(instanceCount, stride, ComputeBufferType.Structured);

        positionBuffer.SetData(positions);
        rotationBuffer.SetData(rotations);
        scaleBuffer.SetData(scales);

        // bind buffers to material (MUST MATCH SHADER)
        runtimeMaterial.SetBuffer("instancePositions", positionBuffer);
        runtimeMaterial.SetBuffer("instanceRotations", rotationBuffer);
        runtimeMaterial.SetBuffer("instanceScales", scaleBuffer);

        //runtimeMaterial.SetInt("_VertexCountPerInstance", grassMesh.vertexCount);
        // --------------- INDIRECT ARGS BUFFER ----------------
        uint[] args = new uint[5];
        args[0] = (grassMesh != null) ? grassMesh.GetIndexCount(0) : 0;  // index count
        args[1] = (uint)instanceCount;                                   // instance count
        args[2] = (grassMesh != null) ? grassMesh.GetIndexStart(0) : 0;
        args[3] = (grassMesh != null) ? grassMesh.GetBaseVertex(0) : 0;
        args[4] = 0;

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        Debug.Log($"VertexCount={grassMesh.vertexCount}, Instances={positions.Length}");
    }

    void Update()
    {
        if (runtimeMaterial == null) return;
        Bounds bounds = new Bounds(center, size);
        bounds.Expand(100);
        Graphics.DrawMeshInstancedIndirect(
            grassMesh,
            0,
            runtimeMaterial,
            bounds,
            argsBuffer
        );
    }

    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        if (rotationBuffer != null) rotationBuffer.Release();
        if (scaleBuffer != null) scaleBuffer.Release();
        if (argsBuffer != null) argsBuffer.Release();
    }
}
