using UnityEngine;

public class Deformer : MonoBehaviour
{
    public TerrainDeformer _deformer;
    public Vector2 craterRadius = new Vector2(4f,6f);
    public Vector2 craterDepth = new Vector2(4f, 6f);
    public AnimationCurve craterFalloff;
    private Rigidbody rb;
    private bool _hasHit = false;

    private void Awake()
    {
        _deformer = FindFirstObjectByType<TerrainDeformer>();
        rb = GetComponent<Rigidbody>(); 
    }

    private void Start()
    {
        rb.AddForce(Camera.main.transform.forward * 20, ForceMode.VelocityChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerStateMachine stateMachine)) return;
        
        if(other.TryGetComponent(out Terrain terrain))
        {
            Debug.Log("asd");
            Explosion(rb.transform.position);
        }
        Destroy(gameObject);
    }

    void Explosion(Vector3 hitPoint)
    {
        float radius = Random.Range(craterRadius.x, craterRadius.y);
        float depth = Random.Range(craterDepth.x, craterDepth.y);
        _deformer.CreateCrater(hitPoint, radius, depth, craterFalloff);
    }
}
