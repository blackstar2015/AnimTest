using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public float _craterRadius {get; private set;} 
    public float _craterDepth { get; private set; }
    public AnimationCurve _craterFalloff {  get; private set; }

    Terrain _terrain;
    TerrainData _terrainData;

    void Start()
    {
        if (_craterFalloff == null || _craterFalloff.keys.Length == 0)
        {
            _craterFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
    }

    public void SetTerrain(Terrain terrain, TerrainData terrainData)
    {
        _terrain = terrain;
        _terrainData = terrainData;
    }

    public void CreateCrater(Vector3 worldPos, float craterRadius, float craterDepth, AnimationCurve craterFalloff)
    {
        if (!_terrain || !_terrainData) return;
        _craterRadius = craterRadius;
        _craterDepth = craterDepth;
        _craterFalloff = craterFalloff;

        Vector3 terrainPos = _terrain.transform.position;
        int res = _terrainData.heightmapResolution;

        // convert world to normalized terrain space
        float normX = (worldPos.x - terrainPos.x) /_terrainData.size.x;
        float normZ = (worldPos.z - terrainPos.z) / _terrainData.size.z;

        // convert normalized to heightmap indices
        int centerX = Mathf.RoundToInt(normX * (res - 1));
        int centerZ = Mathf.RoundToInt(normZ * (res - 1));

        int radiusInPx = Mathf.RoundToInt(craterRadius / _terrainData.size.x * (res - 1));

        int startX = Mathf.Clamp(centerX - radiusInPx, 0, res - 1);
        int startZ = Mathf.Clamp(centerZ - radiusInPx, 0, res - 1);
        int endX = Mathf.Clamp(centerX + radiusInPx, 0, res - 1);
        int endZ = Mathf.Clamp(centerZ + radiusInPx, 0, res - 1);

        int w = endX - startX;
        int h = endZ - startZ;

        float[,] heights = _terrainData.GetHeights(startX, startZ, w, h);

        for (int z = 0; z < h; z++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x + startX - centerX) / (float)radiusInPx;
                float dz = (z + startZ - centerZ) / (float)radiusInPx;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist <= 1f)
                {
                    float falloff = craterFalloff.Evaluate(dist);
                    heights[z, x] -= falloff * (craterDepth / _terrainData.size.y);
                }
            }
        }
        _terrainData.SetHeights(startX, startZ, heights);
    }
}
