using Sirenix.OdinInspector;
using UnityEngine;

public class CustomEnemyController : CustomController
{
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private Transform _target;


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _target = FindFirstObjectByType<CustomPlayerController>().transform;
    }

    protected override void Update()
    {
        base.Update();
        float stopDistance = 1f;
        float distance = Vector3.Distance(transform.position, _target.position);
        if(distance < Weapons[weaponIndex].Data.Range) Weapons[weaponIndex].TryAttack(_target.position,this.gameObject,Targetable.Team);
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
