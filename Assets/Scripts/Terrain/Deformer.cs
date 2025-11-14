using UnityEngine;

public class Deformer : MonoBehaviour
{
    public TerrainDeformer _deformer;
    public float craterRadius = 8f;
    public float craterDepth = 4f;
    public AnimationCurve craterFalloff;

    private void Start()
    {
        _deformer = FindFirstObjectByType<TerrainDeformer>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * 5, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        Explosion(transform.position);
        Destroy(gameObject);
    }


    void Explosion(Vector3 hitPoint)
    {
        _deformer.CreateCrater(hitPoint, craterRadius, craterDepth, craterFalloff);
    }
}
