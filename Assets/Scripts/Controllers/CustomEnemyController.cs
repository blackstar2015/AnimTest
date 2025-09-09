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
        float stopDistance = 1f;
        float distance = Vector3.Distance(transform.position, _target.position);
        if(distance < Weapons[_weaponIndex].Data.Range) Weapons[_weaponIndex].TryAttack(_target.position,this.gameObject,0);
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
