using UnityEngine;

public class TerrainDeformer : MonoBehaviour
{
    public float _craterRadius {get; private set;} 
    public float _craterDepth { get; private set; }
    public AnimationCurve _craterFalloff {  get; private set; }

    Terrain terrain;
    TerrainData terrainData;

    void Start()
    {
        terrain = FindAnyObjectByType<Terrain>();
       
        terrainData = terrain.terrainData;

        if (_craterFalloff == null || _craterFalloff.keys.Length == 0)
        {
            _craterFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
    }

    public void CreateCrater(Vector3 worldPos, float craterRadius, float craterDepth, AnimationCurve craterFalloff)
    {
        _craterRadius = craterRadius;
        _craterDepth = craterDepth;
        _craterFalloff = craterFalloff;
        Vector3 terrainPos = WorldToTerrainCoords(worldPos);

        int hmRes = terrainData.heightmapResolution;
        int size = (int)(_craterRadius * (hmRes / terrainData.size.x));

        int centerX = (int)(terrainPos.x * hmRes);
        int centerZ = (int)(terrainPos.z * hmRes);

        float[,] heights = terrainData.GetHeights(centerX - size, centerZ - size, size * 2, size * 2);

        for (int z = 0; z < size * 2; z++)
        {
            for (int x = 0; x < size * 2; x++)
            {
                float dx = (x - size) / (float)size;
                float dz = (z - size) / (float)size;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);

                if (dist > 1f)
                    continue;

                float impact = _craterFalloff.Evaluate(dist);
                heights[z, x] -= impact * (_craterDepth / terrainData.size.y);
            }
        }

        terrainData.SetHeights(centerX - size, centerZ - size, heights);
    }

    Vector3 WorldToTerrainCoords(Vector3 worldPos)
    {
        Vector3 terrainPos = terrain.transform.position;
        Vector3 relative = worldPos - terrainPos;

        return new Vector3(
            relative.x / terrainData.size.x,
            relative.y / terrainData.size.y,
            relative.z / terrainData.size.z
        );
    }
}
