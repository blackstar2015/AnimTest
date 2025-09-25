using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomEnemyController : CustomController
{
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private Transform _target;
    public bool CanAttackPlayer = false;
    public Spawner spawner;

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
        if (_target.TryGetComponent(out Health health) && health.IsAlive)
        {
            if (distance < Weapons[weaponIndex].Data.Range && CanAttackPlayer) StartCoroutine(AttackRoutine());
        }
        else
        {
            CanAttackPlayer = false;
        }
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

    private IEnumerator AttackRoutine()
    {
        Weapons[weaponIndex].TryAttack(_target.position,this.gameObject,Targetable.Team);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        CanAttackPlayer = false;
        yield return null;
    }
}
