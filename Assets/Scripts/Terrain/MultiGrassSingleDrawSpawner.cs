using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MultiGrassSingleDrawSpawner : MonoBehaviour
{
    [Header("Renderer")]
    public InstancedIndirectGrassRenderer renderer; // single renderer for all grass types

    [Header("Grass Settings")]
    [SerializeField] private float _jitter = 1f;
    [SerializeField] private float _density = 1f;
    [SerializeField] private float _noiseThreshold = 0.5f;

    private Terrain _terrain;
    private TerrainData _terrainData;

    [System.Serializable]
    public struct GrassInstanceData
    {
        public Vector3 pos;
        public uint type;
    }

    private void Update()
    {
        if (_terrainData == null || renderer == null) return;

        // Generate/update grass buffer
        GenerateGrassBuffer();
    }

    public void SetTerrain(Terrain terrain)
    {
        _terrain = terrain;
        _terrainData = terrain.terrainData;
    }

    private void GenerateGrassBuffer()
    {
        int mapW = _terrainData.alphamapWidth;
        int mapH = _terrainData.alphamapHeight;
        float[,,] alphamaps = _terrainData.GetAlphamaps(0, 0, mapW, mapH);

        List<GrassInstanceData> allGrass = new List<GrassInstanceData>();

        // Fixed seed for deterministic placement
        Random.InitState(123);

        for (int y = 0; y < mapH; y++)
        {
            for (int x = 0; x < mapW; x++)
            {
                float normX = (float)x / (mapW - 1);
                float normZ = (float)y / (mapH - 1);

                float worldX = _terrain.transform.position.x + normX * _terrainData.size.x;
                float worldZ = _terrain.transform.position.z + normZ * _terrainData.size.z;

                int attempts = Mathf.CeilToInt(_density);
                for (int j = 0; j < attempts; j++)
                {
                    float rx = (Random.value - 0.5f) * _jitter;
                    float rz = (Random.value - 0.5f) * _jitter;

                    float jitterX = Mathf.Clamp(worldX + rx, _terrain.transform.position.x, _terrain.transform.position.x + _terrainData.size.x);
                    float jitterZ = Mathf.Clamp(worldZ + rz, _terrain.transform.position.z, _terrain.transform.position.z + _terrainData.size.z);

                    float normJitX = (jitterX - _terrain.transform.position.x) / _terrainData.size.x;
                    float normJitZ = (jitterZ - _terrain.transform.position.z) / _terrainData.size.z;

                    float heightJ = _terrainData.GetInterpolatedHeight(normJitX, normJitZ);

                    // Iterate over all terrain layers for multi-type grass
                    for (uint layer = 0; layer < alphamaps.GetLength(2); layer++)
                    {
                        float weight = alphamaps[y, x, layer];
                        float noise = Mathf.PerlinNoise(normJitX * 0.1f, normJitZ * 0.1f);

                        // Spawn grass if texture weight is high and noise passes threshold
                        if (Random.value < weight && noise > _noiseThreshold)
                        {
                            allGrass.Add(new GrassInstanceData
                            {
                                pos = new Vector3(jitterX, heightJ, jitterZ),
                                type = layer
                            });
                        }
                    }
                }
            }
        }

        // Send the generated grass data to the renderer
        if (renderer != null)
        {
            renderer.SetCombinedGrassBuffer(allGrass);
        }
    }
}
