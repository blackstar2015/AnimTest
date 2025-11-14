using System.Drawing;
using UnityEngine;

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

    private Terrain terrain;
    private TerrainData terrainData;

    private void Awake()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);

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

        terrainData.SetHeights(0, 0, heights);
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrain = terrainObject.GetComponent<Terrain>();
        terrain.transform.position = Vector3.zero;
        AssignTerrainLayers();
        PaintTerrainTextures();
    }

    private void AssignTerrainLayers()
    {
        TerrainLayer grass = new TerrainLayer();
        grass.diffuseTexture = Resources.Load<Texture2D>("Textures/Grass_A_BaseColor");
        grass.normalMapTexture = Resources.Load<Texture2D>("Textures/Grass_A_Normal");
        grass.maskMapTexture = Resources.Load<Texture2D>("Textures/Grass_A_MaskMap");

        TerrainLayer rock = new TerrainLayer();
        rock.diffuseTexture = Resources.Load<Texture2D>("Textures/Rock_BaseColor");
        rock.normalMapTexture = Resources.Load<Texture2D>("Textures/Rock_Normal");
        rock.maskMapTexture= Resources.Load<Texture2D>("Textures/Rock_MaskMap");

        TerrainLayer sand = new TerrainLayer();
        sand.diffuseTexture = Resources.Load<Texture2D>("Textures/Sand_BaseColor");
        sand.normalMapTexture = Resources.Load<Texture2D>("Textures/Sand_Normal");
        sand.maskMapTexture = Resources.Load<Texture2D>("Textures/Sand_MaskMap");

        terrainData.terrainLayers = new TerrainLayer[] { grass, rock, sand };
    }

    private void PaintTerrainTextures()
    {
        int alphaRes = terrainData.alphamapResolution;
        int layers = terrainData.alphamapLayers;

        float[,,] splatmap = new float[alphaRes, alphaRes, layers];

        for (int y = 0; y < alphaRes; y++)
        {
            for (int x = 0; x < alphaRes; x++)
            {
                float normX = x / (float)alphaRes;
                float normY = y / (float)alphaRes;

                float height = terrainData.GetInterpolatedHeight(normX, normY) / terrainData.size.y;
                float slope = terrainData.GetSteepness(normX, normY) / 90f;

                float[] weights = new float[layers];

                // Example: Grass = layer 0, Rock = layer 1, Sand = layer 2
                weights[0] = Mathf.Clamp01(1f - slope);   // grass on flat areas
                weights[1] = slope;                       // rock on steep slopes
                weights[2] = Mathf.Clamp01(height * 2f);  // sand at low heights

                // Normalize
                float total = weights[0] + weights[1] + weights[2];
                for (int i = 0; i < layers; i++)
                    weights[i] /= total;

                for (int i = 0; i < layers; i++)
                    splatmap[y, x, i] = weights[i];
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmap);
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

    public TerrainData GetTerrainData() => terrainData;
}
