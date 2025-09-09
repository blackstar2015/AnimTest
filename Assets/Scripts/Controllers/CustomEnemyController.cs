using UnityEngine;

public class CustomEnemyController : CustomController
{
    private Transform _target;

    private void Start()
    {
        _target = FindFirstObjectByType<CustomPlayerController>().transform;
    }

    private void Update()
    {
        float stopDistance = 1.5f;
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance > stopDistance)
        {
            Movement.MoveTo(_target.position);
        }
        else
        {
            Movement.Stop();
            Movement.SetLookPosition(_target.position);
        }
    }
}
