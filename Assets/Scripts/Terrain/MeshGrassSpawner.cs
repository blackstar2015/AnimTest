using UnityEngine;
using System.Collections.Generic;
[DefaultExecutionOrder(1)]
public class MeshGrassSpawner : MonoBehaviour
{
    [Header("Assign")]
    public Mesh grassMesh;
    public Material grassMaterial; // assign your material asset (shader = Custom/GrassURPIndirect)
    public Terrain terrain;
    public int density = 1;

    // runtime
    Material runtimeMat;
    ComputeBuffer posBuffer, rotBuffer, scaleBuffer, argsBuffer;
    Bounds drawBounds;
    int instanceCount = 0;
    bool buffersReady = false;

    void Start()
    {
        if (terrain == null) terrain = Terrain.activeTerrain;
        if (grassMesh == null || grassMaterial == null || terrain == null)
        {
            Debug.LogError("Missing references on GPUGrassSpawnerFixed.");
            enabled = false;
            return;
        }

        // Create runtime material instance (DO THIS ONCE)
        runtimeMat = new Material(grassMaterial);

        // build instances and GPU buffers
        CreateAndUploadBuffers();

        // set vertex count on runtime material
        int vcount = grassMesh.vertexCount;
        runtimeMat.SetInt("_VertexCountPerInstance", vcount);

        // bind buffers to runtime material
        runtimeMat.SetBuffer("instancePositions", posBuffer);
        runtimeMat.SetBuffer("instanceRotations", rotBuffer);
        runtimeMat.SetBuffer("instanceScales", scaleBuffer);

        // build draw bounds (slightly larger to avoid culling)
        Vector3 size = terrain.terrainData.size * 1.1f;
        drawBounds = new Bounds(terrain.transform.position + terrain.terrainData.size / 2f, size);

        buffersReady = true;

        Debug.Log($"Buffers ready. instances={instanceCount} vertexCount={vcount} runtimeMat==grassMaterial? {ReferenceEquals(runtimeMat, grassMaterial)}");
    }

    void CreateAndUploadBuffers()
    {
        // sample positions
        List<Vector3> posList = new List<Vector3>();
        List<Vector3> rotList = new List<Vector3>(); // store euler.y in .y, other channels unused
        List<float> scaleList = new List<float>();

        Vector3 terrainSize = terrain.terrainData.size;
        for (int x = 0; x < (int)terrainSize.x; x += density)
        {
            for (int z = 0; z < (int)terrainSize.z; z += density)
            {
                float nx = x / terrainSize.x;
                float nz = z / terrainSize.z;
                float y = terrain.terrainData.GetInterpolatedHeight(nx, nz) + terrain.transform.position.y;

                posList.Add(new Vector3(x, y, z) + terrain.transform.position);
                float ry = Random.Range(0f, 360f);
                rotList.Add(new Vector3(0f, ry, 0f));
                scaleList.Add(Random.Range(0.8f, 1.2f));
            }
        }

        instanceCount = posList.Count;
        if (instanceCount == 0)
        {
            Debug.LogWarning("No grass instances generated.");
            return;
        }

        // release old buffers if any
        posBuffer?.Release();
        rotBuffer?.Release();
        scaleBuffer?.Release();
        argsBuffer?.Release();

        posBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        rotBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 3);
        scaleBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 1);

        posBuffer.SetData(posList);
        rotBuffer.SetData(rotList);
        scaleBuffer.SetData(scaleList);

        // args buffer
        uint[] args = new uint[5];
        args[0] = (uint)grassMesh.GetIndexCount(0);
        args[1] = (uint)instanceCount;
        args[2] = (uint)grassMesh.GetIndexStart(0);
        args[3] = (uint)grassMesh.GetBaseVertex(0);
        args[4] = 0;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        Debug.Log($"Created buffers: pos({posBuffer.count}), rot({rotBuffer.count}), scale({scaleBuffer.count}), args.instances={args[1]}");
    }

    void Update()
    {
        if (!buffersReady) return;

        // extra debug to ensure runtimeMat is what we will use
        if (runtimeMat == null)
        {
            Debug.LogError("runtimeMat is null in Update.");
            return;
        }

        // final draw — MUST use runtimeMat here (not grassMaterial)
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, runtimeMat, drawBounds, argsBuffer);
    }

    void OnDisable()
    {
        posBuffer?.Release();
        rotBuffer?.Release();
        scaleBuffer?.Release();
        argsBuffer?.Release();
    }
}
