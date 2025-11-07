using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyStateMachine : StateMachine
{
    [field: SerializeField, TabGroup("Properties")] public CustomEnemyController EnemyController=> Controller as CustomEnemyController;
    [field: SerializeField, TabGroup("Properties")] public bool DebugStateTransitions;
    public Vector3 PatrolPoint;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float EnemyAttackSpeedMultiplier = .5f;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float EnemyWalkSpeedMultiplier = 1f;

    public override void Awake()
    {
        base.Awake();
        EnemyController.SetStateMachine(this);
        SwitchState(new EnemyIdleState(this, true));
    }
    public override void Update()
    {
        base.Update();
        _currentState?.Tick(Time.deltaTime);
    }

    public Vector3 GetPatrolPoint()
    {
        PatrolPoint = (EnemyController.GetSpawnTransform().position + Random.insideUnitSphere * 15);
        PatrolPoint.y = .1f;
        return PatrolPoint;
    }

    public override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + Vector3.up, PatrolPoint + Vector3.up);
        Gizmos.DrawSphere(PatrolPoint, 1f);
    }
}
