//see this for ref: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

[ExecuteAlways]
public class InstancedIndirectGrassRenderer : MonoBehaviour
{
    [Header("Settings")]
    public float drawDistance = 125;//this setting will affect performance a lot!
    [Range(1, 4000)]
    [SerializeField] public int InstanceCount = 100;
    public Material instanceMaterial;

    [Header("Internal")]
    public ComputeShader cullingComputeShader;

    [NonSerialized]
    public List<Vector3> allGrassPos = new List<Vector3>();//user should update this list using C#
    //=====================================================
    [HideInInspector]   
    public static InstancedIndirectGrassRenderer instance;// global ref to this script


    private int cellCountX = -1;
    private int cellCountZ = -1;
    private int dispatchCount = -1;

    //smaller the number, CPU needs more time, but GPU is faster
    private float cellSizeX = 10; //unity unit (m)
    private float cellSizeZ = 10; //unity unit (m)

    private int instanceCountCache = -1;
    private Mesh cachedGrassMesh;

    private ComputeBuffer allInstancesPosWSBuffer;
    private ComputeBuffer visibleInstancesOnlyPosWSIDBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer visibleCountBuffer;

    private List<Vector3>[] cellPosWSsList; //for binning: binning will put each posWS into correct cell
    private float minX, minZ, maxX, maxZ;
    private List<int> visibleCellIDList = new List<int>();
    private Plane[] cameraFrustumPlanes = new Plane[6];

    [SerializeField] bool shouldBatchDispatch = true;
    private Terrain _terrain;
    private TerrainData _terrainData;
    private float _terrainHeight;

    //=====================================================

    private void OnEnable()
    {
        instance = this; // assign global ref using this script
    }

    //assign terrain data from ProceduralTerrainGenerator
    public void SetTerrain(Terrain terrain, TerrainData terrainData, float terrainHeight)
    {
        _terrain = terrain;
        _terrainData = terrainData;
        _terrainHeight = terrainHeight;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (_terrain == null || _terrainData == null) return;

        // Update all buffers if instance list changed
        UpdateAllInstanceTransformBufferIfNeeded();

        // Build VP matrix
        Matrix4x4 vp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;

        // Reset append buffer counter
        visibleInstancesOnlyPosWSIDBuffer.SetCounterValue(0);

        cullingComputeShader.SetMatrix("_VPMatrix", vp);
        cullingComputeShader.SetFloat("_MaxDrawDistance", drawDistance);

        const int THREADS = 64;
        int groups = Mathf.CeilToInt(allGrassPos.Count / (float)THREADS);

        // Dispatch single group covering all instances
        cullingComputeShader.SetInt("_StartOffset", 0);
        cullingComputeShader.Dispatch(0, Mathf.Max(1, groups), 1, 1);

        // Update args buffer with count of visible instances
        ComputeBuffer.CopyCount(visibleInstancesOnlyPosWSIDBuffer, argsBuffer, 4);

        // Render all visible instances
        Vector3 center = _terrain.transform.position + _terrainData.size * 0.5f;
        Vector3 size = _terrainData.size;
        Bounds renderBound = new Bounds(center, size * 1.2f); // slightly larger than terrain

        Graphics.DrawMeshInstancedIndirect(GetGrassMeshCache(), 0, instanceMaterial, renderBound, argsBuffer);
    }

    void OnDisable()
    {
        //release all compute buffers
        if (allInstancesPosWSBuffer != null)
            allInstancesPosWSBuffer.Release();
        allInstancesPosWSBuffer = null;

        if (visibleInstancesOnlyPosWSIDBuffer != null)
            visibleInstancesOnlyPosWSIDBuffer.Release();
        visibleInstancesOnlyPosWSIDBuffer = null;

        if (visibleCountBuffer != null)
            visibleCountBuffer.Release();
        visibleCountBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;

        instance = null;
    }

    Mesh GetGrassMeshCache()
    {
        if (!cachedGrassMesh)
        {
            //if not exist, create a 3 vertices hardcode triangle grass mesh
            cachedGrassMesh = new Mesh();

            //single grass (vertices)
            Vector3[] verts = new Vector3[3];
            verts[0] = new Vector3(-0.25f, 0);
            verts[1] = new Vector3(+0.25f, 0);
            verts[2] = new Vector3(-0.0f, 1);
            //single grass (Triangle index)
            int[] trinagles = new int[3] { 2, 1, 0, }; //order to fit Cull Back in grass shader

            cachedGrassMesh.SetVertices(verts);
            cachedGrassMesh.SetTriangles(trinagles, 0);
        }

        return cachedGrassMesh;
    }

    void UpdateAllInstanceTransformBufferIfNeeded()
    {
        //always update
        instanceMaterial.SetVector("_PivotPosWS", transform.position);
        instanceMaterial.SetVector("_BoundSize", new Vector2(transform.localScale.x, transform.localScale.z));

        //early exit if no need to update buffer
        if (instanceCountCache == allGrassPos.Count &&
            argsBuffer != null &&
            allInstancesPosWSBuffer != null &&
            visibleInstancesOnlyPosWSIDBuffer != null)
            {
                return;
            }

        Debug.Log("UpdateAllInstanceTransformBuffer (Slow)");

        ///////////////////////////
        // allInstancesPosWSBuffer buffer
        ///////////////////////////
        if (allInstancesPosWSBuffer != null)
            allInstancesPosWSBuffer.Release();
        int bufferSize = Mathf.CeilToInt(MathF.Max(1f, allGrassPos.Count));
        allInstancesPosWSBuffer = new ComputeBuffer(bufferSize, sizeof(float)*3); //float3 posWS only, per grass

        if (visibleInstancesOnlyPosWSIDBuffer != null)
        visibleInstancesOnlyPosWSIDBuffer.Release();
        visibleInstancesOnlyPosWSIDBuffer = new ComputeBuffer(bufferSize, sizeof(uint), ComputeBufferType.Append); //uint only, per visible grass
        
        if (visibleCountBuffer != null)
        visibleCountBuffer.Release();
        visibleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Default);

        //find all instances's posWS XZ bound min max
        minX = float.MaxValue;
        minZ = float.MaxValue;
        maxX = float.MinValue;
        maxZ = float.MinValue;
        for (int i = 0; i < allGrassPos.Count; i++)
        {
            Vector3 target = allGrassPos[i];
            minX = Mathf.Min(target.x, minX);
            minZ = Mathf.Min(target.z, minZ);
            maxX = Mathf.Max(target.x, maxX);
            maxZ = Mathf.Max(target.z, maxZ);
        }

        //decide cellCountX,Z here using min max
        //each cell is cellSizeX x cellSizeZ
        cellCountX = Mathf.CeilToInt((maxX - minX) / cellSizeX); 
        cellCountZ = Mathf.CeilToInt((maxZ - minZ) / cellSizeZ);

        //init per cell posWS list memory
        cellPosWSsList = new List<Vector3>[cellCountX * cellCountZ]; //flatten 2D array
        for (int i = 0; i < cellPosWSsList.Length; i++)
        {
            cellPosWSsList[i] = new List<Vector3>();
        }

        //binning, put each posWS into the correct cell
        for (int i = 0; i < allGrassPos.Count; i++)
        {
            Vector3 pos = allGrassPos[i];

            //find cellID
            int xID = Mathf.Min(cellCountX-1,Mathf.FloorToInt(Mathf.InverseLerp(minX, maxX, pos.x) * cellCountX)); //use min to force within 0~[cellCountX-1]  
            int zID = Mathf.Min(cellCountZ-1,Mathf.FloorToInt(Mathf.InverseLerp(minZ, maxZ, pos.z) * cellCountZ)); //use min to force within 0~[cellCountZ-1]

            cellPosWSsList[xID + zID * cellCountX].Add(pos);
        }

        //combine to a flatten array for compute buffer
        int offset = 0;
        Vector3[] allGrassPosWSSortedByCell = new Vector3[allGrassPos.Count];
        for (int i = 0; i < cellPosWSsList.Length; i++)
        {
            for (int j = 0; j < cellPosWSsList[i].Count; j++)
            {
                allGrassPosWSSortedByCell[offset] = cellPosWSsList[i][j];
                offset++;
            }
        }

        allInstancesPosWSBuffer.SetData(allGrassPosWSSortedByCell);
        instanceMaterial.SetBuffer("_AllInstancesTransformBuffer", allInstancesPosWSBuffer);
        instanceMaterial.SetBuffer("_VisibleInstanceOnlyTransformIDBuffer", visibleInstancesOnlyPosWSIDBuffer);

        ///////////////////////////
        // Indirect args buffer
        ///////////////////////////
        if (argsBuffer != null)
            argsBuffer.Release();
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

        args[0] = (uint)GetGrassMeshCache().GetIndexCount(0);
        args[1] = (uint)InstanceCount;
        args[2] = (uint)GetGrassMeshCache().GetIndexStart(0);
        args[3] = (uint)GetGrassMeshCache().GetBaseVertex(0);
        args[4] = 0;

        argsBuffer.SetData(args);
        Debug.Log($"ARGS before Draw: indexCount={args[0]} instanceCount={args[1]} startIndex={args[2]} baseVertex={args[3]} startInstance={args[4]}");
        ///////////////////////////
        // Update Cache
        ///////////////////////////
        //update cache to prevent future no-op buffer update, which waste performance
        instanceCountCache = allGrassPos.Count;

        //set buffer
        cullingComputeShader.SetBuffer(0, "_AllInstancesPosWSBuffer", allInstancesPosWSBuffer);
        cullingComputeShader.SetBuffer(0, "_VisibleInstancesOnlyPosWSIDBuffer", visibleInstancesOnlyPosWSIDBuffer);
        cullingComputeShader.SetBuffer(0, "_VisibleCount", visibleCountBuffer);
    }

    public void SetCombinedGrassBuffer(List<MultiGrassSingleDrawSpawner.GrassInstanceData> instances)
    {
        if (allInstancesPosWSBuffer != null)
            allInstancesPosWSBuffer.Release();

        // Convert to GPU struct
        GrassInstanceGPU[] gpuData = new GrassInstanceGPU[instances.Count];
        for (int i = 0; i < instances.Count; i++)
        {
            gpuData[i].pos = instances[i].pos;
            gpuData[i].type = instances[i].type;
        }

        // Upload buffer
        allInstancesPosWSBuffer = new ComputeBuffer(gpuData.Length, sizeof(float) * 3 + sizeof(uint));
        allInstancesPosWSBuffer.SetData(gpuData);

        // Bind to material
        instanceMaterial.SetBuffer("_AllInstancesTransformBuffer", allInstancesPosWSBuffer);

        // Setup argsBuffer for DrawMeshInstancedIndirect
        Mesh mesh = GetGrassMeshCache();
        uint[] args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)gpuData.Length; // number of instances
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        args[4] = 0;

        if (argsBuffer == null)
            argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        argsBuffer.SetData(args);
    }
    public struct GrassInstanceGPU
    {
        public Vector3 pos;
        public uint type;
    }
}