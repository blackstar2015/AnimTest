using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class InstancedIndirectGrassPosDefine : MonoBehaviour
{
    [Range(1000000, 40000000)]
    public int instanceCount = 1000000;
    public float drawDistance = 125;
    [SerializeField]private float _offset = -100f;
    private int cacheCount = -1;

    private Terrain _terrain;
    private TerrainData _terrainData;

    private void Start()
    {        
        UpdatePosIfNeeded();
    }
    private void Update()
    {
        UpdatePosIfNeeded();
    }

    public void SetTerrain(Terrain terrain, TerrainData terrainData)
    {
        _terrain = terrain;
        _terrainData = terrainData;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(300, 50, 200, 30), "Instance Count: " + instanceCount / 1000000 + "Million");
        instanceCount = Mathf.Max(1, (int)(GUI.HorizontalSlider(new Rect(300, 100, 200, 30), instanceCount / 1000000f, 1, 10)) * 1000000);

        GUI.Label(new Rect(300, 150, 200, 30), "Draw Distance: " + drawDistance);
        drawDistance = Mathf.Max(1, (int)(GUI.HorizontalSlider(new Rect(300, 200, 200, 30), drawDistance / 25f, 1, 8)) * 25);
        InstancedIndirectGrassRenderer.instance.drawDistance = drawDistance;
    }
    private void UpdatePosIfNeeded()
    {
        if (instanceCount == cacheCount || _terrainData == null)
            return;

        Debug.Log("UpdatePos (Slow)");

        //same seed to keep grass visual the same
        UnityEngine.Random.InitState(123);

        //auto keep density the same
        float scale = Mathf.Sqrt((instanceCount / 4)) / 2f;
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);

        //////////////////////////////////////////////////////////////////////////
        //can define any posWS in this section, random is just an example
        //////////////////////////////////////////////////////////////////////////
        List<Vector3> positions = new List<Vector3>(instanceCount);
        for (int i = 0; i < instanceCount; i++)
        {            
            Vector3 pos = Vector3.zero;
            pos.x = UnityEngine.Random.Range(-1f, 1f) * transform.lossyScale.x;
            pos.z = UnityEngine.Random.Range(-1f, 1f) * transform.lossyScale.z;
            pos.y = _terrainData.GetHeight((int)pos.x,(int)pos.z) + _offset;
            pos += transform.position;
            positions.Add(new Vector3(pos.x, pos.y, pos.z));
        }
        //send all posWS to renderer
        InstancedIndirectGrassRenderer.instance.allGrassPos = positions;
        cacheCount = positions.Count;
    }
}
