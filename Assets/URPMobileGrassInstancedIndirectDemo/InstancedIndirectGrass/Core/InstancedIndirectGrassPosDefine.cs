using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class InstancedIndirectGrassPosDefine : MonoBehaviour
{
    [Range(1, 4000)]
    [SerializeField] private int _instanceCount => InstancedIndirectGrassRenderer.instance.InstanceCount;
    [SerializeField] private float _drawDistance => InstancedIndirectGrassRenderer.instance.drawDistance;
    [SerializeField] private float _jitter;
    [SerializeField] private float _density;
    [SerializeField] private float _noiseThreshold;
    [SerializeField] private float frequency = 2.0f;
    private int _cacheCount = -1;
    private Terrain _terrain;
    private TerrainData _terrainData;

    private void Start()
    {        
        UpdatePosIfNeeded();
    }
    private void Update()
    {
        //UpdatePosIfNeeded();
    }

    public void SetTerrain(Terrain terrain, TerrainData terrainData)
    {
        _terrain = terrain;
        _terrainData = terrainData;
    }

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(300, 50, 200, 30), "Instance Count: " + instanceCount / 1000000 + "Million");
    //    instanceCount = Mathf.Max(1, (int)(GUI.HorizontalSlider(new Rect(300, 100, 200, 30), instanceCount / 1000000f, 1, 10)) * 1000000);

    //    GUI.Label(new Rect(300, 150, 200, 30), "Draw Distance: " + drawDistance);
    //    drawDistance = Mathf.Max(1, (int)(GUI.HorizontalSlider(new Rect(300, 200, 200, 30), drawDistance / 25f, 1, 8)) * 25);
    //    InstancedIndirectGrassRenderer.instance.drawDistance = drawDistance;
    //}
    private void UpdatePosIfNeeded()
    {
        if (_instanceCount == _cacheCount || _terrainData == null)
            return;

        Debug.Log("UpdatePos (Slow)");

        //same seed to keep grass visual the same
        UnityEngine.Random.InitState(123);

        //auto keep density the same
        float scale = Mathf.Sqrt((_instanceCount / 4)) / 2f;
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);

        //define any posWS in this section
        List<Vector3> positions = new List<Vector3>(_instanceCount);
       
        int mapW = _terrainData.alphamapWidth;
        int mapH = _terrainData.alphamapHeight;

        float[,,] alphamaps = _terrainData.GetAlphamaps(0, 0, mapW, mapH);

        int targetLayer = 0; // the splat layer you want grass from
        float threshold = 0.5f;


        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                float weight = alphamaps[y, x, targetLayer];
                if (weight < threshold)
                    continue;

                // normalized coordinates
                float normX = (float)x / (mapW - 1);
                float normZ = (float)y / (mapH - 1);

                float worldX = _terrain.transform.position.x + normX * _terrainData.size.x;
                float worldZ = _terrain.transform.position.z + normZ * _terrainData.size.z;

                // generate jittered positions
                int attempts = Mathf.CeilToInt(_density); // number of random points per pixel
                for (int j = 0; j < attempts; j++)
                {
                    float rx = (Random.value - 0.5f) * _jitter;
                    float rz = (Random.value - 0.5f) * _jitter;

                    float jitterX = Mathf.Clamp(worldX + rx, _terrain.transform.position.x, _terrain.transform.position.x + _terrainData.size.x);
                    float jitterZ = Mathf.Clamp(worldZ + rz, _terrain.transform.position.z, _terrain.transform.position.z + _terrainData.size.z);

                    float normJitX = (jitterX - _terrain.transform.position.x) / _terrainData.size.x;
                    float normJitZ = (jitterZ - _terrain.transform.position.z) / _terrainData.size.z;
                    float heightJ = _terrainData.GetInterpolatedHeight(normJitX, normJitZ);

                    float noise = Mathf.PerlinNoise(normJitX * frequency, normJitZ * frequency);
                    if (noise > _noiseThreshold || Random.value < 0.05f)
                    {
                        positions.Add(new Vector3(jitterX, heightJ, jitterZ));
                    }
                }
            }
        }

        //send all posWS to renderer
        InstancedIndirectGrassRenderer.instance.allGrassPos = positions;
        _cacheCount = positions.Count;
    }
}
