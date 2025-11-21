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
        // recreate all buffers if needed
        UpdateAllInstanceTransformBufferIfNeeded();

        //=====================================================================================================
        // rough quick big cell frustum culling in CPU first
        //=====================================================================================================
        visibleCellIDList.Clear();//fill in this cell ID list using CPU frustum culling first
        Camera cam = Camera.main;

        //Do frustum culling using per cell bound
        //https://docs.unity3d.com/ScriptReference/GeometryUtility.CalculateFrustumPlanes.html
        //https://docs.unity3d.com/ScriptReference/GeometryUtility.TestPlanesAABB.html
        float cameraOriginalFarPlane = cam.farClipPlane;
        cam.farClipPlane = drawDistance;//allow drawDistance control    
        GeometryUtility.CalculateFrustumPlanes(cam, cameraFrustumPlanes);//Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        cam.farClipPlane = cameraOriginalFarPlane;//revert far plane edit

        //slow loop
        //TODO: (A)replace this forloop by a quadtree test?
        //TODO: (B)convert this forloop to job+burst? (UnityException: TestPlanesAABB can only be called from the main thread.)
        Profiler.BeginSample("CPU cell frustum culling (heavy)");
        
        for (int i = 0; i < cellPosWSsList.Length; i++)
        {
            //create cell bound
            Vector3 centerPosWS = new Vector3 (i % cellCountX + 0.5f, 0, i / cellCountX + 0.5f);
            centerPosWS.x = Mathf.Lerp(minX, maxX, centerPosWS.x / cellCountX);
            centerPosWS.z = Mathf.Lerp(minZ, maxZ, centerPosWS.z / cellCountZ);
            Vector3 sizeWS = new Vector3(Mathf.Abs(maxX - minX) / cellCountX,0,Mathf.Abs(maxX - minX) / cellCountX);
            Bounds cellBound = new Bounds(centerPosWS, sizeWS);
            cellBound.size *= 1000f;
            if (GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, cellBound))
            {
                visibleCellIDList.Add(i);
            }
        }
        Profiler.EndSample();

        //=====================================================================================================
        // then loop though only visible cells, each visible cell dispatch GPU culling job once
        // at the end compute shader will fill all visible instance into visibleInstancesOnlyPosWSIDBuffer
        //=====================================================================================================
        
        // ---------- BEFORE dispatch loop: build VP matrix in the convention compute expects ----------
        Matrix4x4 v = cam.worldToCameraMatrix;
        Matrix4x4 p = cam.projectionMatrix;

        // The repo's compute shader comment said "OpenGL standard projection matrix".
        // GL.GetGPUProjectionMatrix lets us request the GPU projection matrix in the desired convention.
        // Try 'false' first (OpenGL-like). If near-camera culling is inverted, try 'true' instead.
        Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(p, false);
        Matrix4x4 vp = gpuProj * v;

        visibleInstancesOnlyPosWSIDBuffer.SetCounterValue(0);

        // bind common compute shader params (already in your code)
        cullingComputeShader.SetMatrix("_VPMatrix", vp);
        cullingComputeShader.SetFloat("_MaxDrawDistance", drawDistance);

        // ---------- dispatch per visible cell ----------
        dispatchCount = 0;
        const int THREADS_PER_GROUP = 64;

        for (int i = 0; i < visibleCellIDList.Count; i++)
        {
            int targetCellFlattenID = visibleCellIDList[i];

            // compute memory offset for this cell (same as your code)
            int memoryOffset = 0;
            for (int j = 0; j < targetCellFlattenID; j++)
                memoryOffset += cellPosWSsList[j].Count;

            cullingComputeShader.SetInt("_StartOffset", memoryOffset);

            int jobLength = cellPosWSsList[targetCellFlattenID].Count;

            // batch neighboring cells into one dispatch (your existing batching)
            if (shouldBatchDispatch)
            {
                while ((i < visibleCellIDList.Count - 1) && (visibleCellIDList[i + 1] == visibleCellIDList[i] + 1))
                {
                    jobLength += cellPosWSsList[visibleCellIDList[i + 1]].Count;
                    i++;
                }
            }

            // --- SAFETY: skip empty dispatches ---
            if (jobLength <= 0)
            {
                // nothing to dispatch for this batch
                continue;
            }

            int groups = Mathf.CeilToInt(jobLength / (float)THREADS_PER_GROUP);
            // ensure >= 1 to satisfy Compute.Dispatch
            groups = Mathf.Max(1, groups);

            // Optional debug — remove or comment out later
#if UNITY_EDITOR
            //Debug.Log($"Dispatch cell batch startOffset={memoryOffset} jobLength={jobLength} groups={groups}");
#endif
            // Reset counter to 0 BEFORE dispatch
            int[] zero = new int[1] { 0 };
            visibleCountBuffer.SetData(zero);

            // Also reset the append buffer counter as you already do
            visibleInstancesOnlyPosWSIDBuffer.SetCounterValue(0);

            // Pass the max instances cap to compute
            cullingComputeShader.SetInt("_MaxInstances", InstanceCount);
            cullingComputeShader.SetMatrix("_VPMatrix", vp);
            cullingComputeShader.SetFloat("_MaxDrawDistance", drawDistance);

            cullingComputeShader.Dispatch(0, groups, 1, 1);
            dispatchCount++;
        }

        //====================================================================================
        // Final 1 big DrawMeshInstancedIndirect draw call 
        //====================================================================================
        // GPU per instance culling finished, copy visible count to argsBuffer, to setup DrawMeshInstancedIndirect's draw amount 

        ComputeBuffer.CopyCount(visibleInstancesOnlyPosWSIDBuffer, argsBuffer, 4);
        if (_terrain == null || _terrainData == null) return;
        // Render 1 big drawcall using DrawMeshInstancedIndirect
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        Vector3 size = terrainData.size;
        Vector3 center = terrain.transform.position + size * 0.5f;
        
        Bounds renderBound = new Bounds();
        renderBound.center = center;
        renderBound.size = new Vector3(size.x + 100f, size.y + _terrainHeight, size.z + 100f) * 100f;
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
}