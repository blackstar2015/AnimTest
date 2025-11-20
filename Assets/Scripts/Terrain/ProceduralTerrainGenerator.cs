using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;

public class ProceduralTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int heightmapResolution = 513;
    public int terrainSize = 1000;
    public int terrainHeight = 80;

    [Header("Noise Settings")]
    public float baseScale = 200f; //size of the large features
    public int octaves = 4; //how many layers of detail you want
    public float lacunarity = 2f; //how quickly each layer gets more detailed
    public float persistence = 0.4f; //how strong each added layer appears
    public Vector2 offset;

    private Terrain _terrain;
    private TerrainData _terrainData;
    [SerializeField] private TerrainDeformer _terrainDeformer;
    [SerializeField] private InstancedIndirectGrassPosDefine _grassPosDefine;
    private void Awake()
    {
        if(_terrainDeformer == null) _terrainDeformer = GetComponent<TerrainDeformer>();
        if(_grassPosDefine == null) _grassPosDefine = FindFirstObjectByType<InstancedIndirectGrassPosDefine>();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        _terrainData = new TerrainData();
        _terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);
        _terrainData.heightmapResolution = heightmapResolution;
        _terrainData.alphamapResolution = 512;
        float[,] heights = GenerateHeightMap();
        _terrainData.SetHeights(0, 0, heights);
        GameObject terrainObject = Terrain.CreateTerrainGameObject(_terrainData);
        _terrain = terrainObject.GetComponent<Terrain>();
        //_terrain.transform.position = Vector3.zero + new Vector3(offset.x,offset.y,0);
        _terrainData.terrainLayers = AssignTerrainLayers();
        PaintTerrainTextures();
        _terrainDeformer.SetTerrain(_terrain, _terrainData);
        _grassPosDefine.SetTerrain(_terrain, _terrainData);
    }
    

    private float[,] GenerateHeightMap()
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];
        for (int z = 0; z < heightmapResolution; z++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                float nx = (float)x / heightmapResolution;
                float nz = (float)z / heightmapResolution;

                heights[z, x] = GenerateFBM(nx, nz);
            }
        }
        return heights;
    }
   
    private TerrainLayer[] AssignTerrainLayers()
    {
        TerrainLayer grass = new TerrainLayer();
        grass.diffuseTexture = Resources.Load<Texture2D>("Textures/Grass_A_BaseColor");
        grass.normalMapTexture = Resources.Load<Texture2D>("Textures/Grass_A_Normal");
        grass.maskMapTexture = Resources.Load<Texture2D>("Textures/Grass_A_MaskMap");
        grass.tileSize = new Vector2(12, 12);
        grass.normalScale = 3;

        TerrainLayer rock = new TerrainLayer();
        rock.diffuseTexture = Resources.Load<Texture2D>("Textures/Rock_BaseColor");
        rock.normalMapTexture = Resources.Load<Texture2D>("Textures/Rock_Normal");
        rock.maskMapTexture= Resources.Load<Texture2D>("Textures/Rock_MaskMap");
        rock.tileSize = new Vector2(8, 8);
        rock.normalScale = 2;

        TerrainLayer sand = new TerrainLayer();
        sand.diffuseTexture = Resources.Load<Texture2D>("Textures/Sand_BaseColor");
        sand.normalMapTexture = Resources.Load<Texture2D>("Textures/Sand_Normal");
        sand.maskMapTexture = Resources.Load<Texture2D>("Textures/Sand_MaskMap");
        sand.tileSize = new Vector2(10, 10);
        sand.normalScale = 2;

        return new TerrainLayer[] { grass, rock, sand };
    }

    private void PaintTerrainTextures()
    {
        int alphaRes = _terrainData.alphamapResolution;
        int layers = _terrainData.alphamapLayers;

        float[,,] splatmap = new float[alphaRes, alphaRes, layers];

        for (int y = 0; y < alphaRes; y++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                float normX = x / (float)alphaRes;
                float normY = y / (float)alphaRes;

                float height = _terrainData.GetInterpolatedHeight(normX, normY) / _terrainData.size.y;
                float slope = _terrainData.GetSteepness(normX, normY) / 90f;
                float noise = Mathf.PerlinNoise(normX * 8f, normY * 8f);

                float[] weights = new float[layers];

                // Example: Grass = layer 0, Rock = layer 1, Sand = layer 2
                // Sand at low elevations
                float sandStrength = Mathf.Clamp01((.4f - height) * 5f);
                sandStrength *= (0.5f + noise * 0.5f); // breakup with noise
                weights[2] = sandStrength;

                // Rock on steep slopes
                float rockStrength = Mathf.Clamp01(slope * 2f);
                rockStrength *= (0.5f + noise * 0.5f);
                weights[1] = rockStrength;

                // Grass everywhere else
                float grassStrength = Mathf.Clamp01(1f - slope) * (1f - sandStrength);
                grassStrength *= (0.5f + noise * 0.5f);
                weights[0] = grassStrength;

                // Normalize
                float total = weights[0] + weights[1] + weights[2];
                for (int i = 0; i < layers; i++)
                    weights[i] /= total;

                for (int i = 0; i < layers; i++)
                    splatmap[y, x, i] = weights[i];
            }
        }
        _terrainData.SetAlphamaps(0, 0, splatmap);
    }

    private float GenerateFBM(float x, float y) //FBM = Fractal Brownian Motion: stack multiple layers of noise on top of each other.
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            total += amplitude * Mathf.PerlinNoise(
                (x * baseScale * frequency) + offset.x,
                (y * baseScale * frequency) + offset.y
            );

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return total * 0.5f;
    }

    public TerrainData GetTerrainData() => _terrainData;
}
