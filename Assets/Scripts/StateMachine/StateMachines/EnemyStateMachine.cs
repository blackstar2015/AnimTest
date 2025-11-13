using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyStateMachine : StateMachine
{
    [SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Properties")] public CustomEnemyController EnemyController=> Controller as CustomEnemyController;


    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Properties")] public bool DebugStateTransitions;
    
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float EnemyAttackSpeedMultiplier = .5f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float EnemyWalkSpeedMultiplier = 1f;

    [FoldoutGroup("States"), TabGroup("States/Tabs","Patrol")] public Vector3 PatrolPoint = Vector3.positiveInfinity;
    [FoldoutGroup("States"), TabGroup("States/Tabs", "Chase")] public float MaxChaseDistance = 5f;
    [FoldoutGroup("States"), TabGroup("States/Tabs", "Chase")] public float ChaseStoppingDistance = 3f;


    public override void Awake()
    {
        base.Awake();
        EnemyController.SetStateMachine(this);
        SwitchState(new EnemyIdleState(this, true));
    }
    private void OnDisable()
    {
        Health.OnDamage.RemoveListener(AddCurrentTarget);
    }
    private void AddCurrentTarget(DamageInfo damageInfo)
    {
        if (damageInfo.Instigator.TryGetComponent(out Targetable targetable))
        {
            CurrentTarget = targetable;
        }
     }

    public override void Update()
    {
        base.Update();
        _currentState?.Tick(Time.deltaTime);
        Targetable currentTarget;
        if (Vision.GetVisibleTargets(Team) != null && CurrentTarget == null)
        {
            currentTarget = Vision.GetFirstVisibleTarget(Team);
            CurrentTarget = currentTarget;
            PreviousTarget = currentTarget;
        }
        else if(Vision.GetVisibleTargets(Team) == null && CurrentTarget != null)
        {
            CurrentTarget = PreviousTarget;
        }
    }

    public Vector3 GetPatrolPoint()
    {
        bool validPatrolPoint = false;
        Vector3 oldPatrolPoint = PatrolPoint;
        while(!validPatrolPoint)
        {
            PatrolPoint = (EnemyController.GetSpawnTransform().position + Random.insideUnitSphere * 15);
            //Magic number to get the ground height
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, GroundMask))
            {
                PatrolPoint.y = hit.point.y;
            }
            if (Vector3.Distance(oldPatrolPoint, PatrolPoint) >= 3f) validPatrolPoint = true;
        }
        return PatrolPoint;
    }

    public override void OnDrawGizmosSelected()
    {
        //Patrol Points
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up, PatrolPoint + Vector3.up);
        Gizmos.DrawSphere(PatrolPoint, 1f);

        //What can the enemy see
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(LookPosition, Range);
        Gizmos.DrawRay(LookPosition, transform.rotation * Quaternion.Euler(0f, FOV * 0.5f, 0f) * Vector3.forward * Range);
        Gizmos.DrawRay(LookPosition, transform.rotation * Quaternion.Euler(0f, -FOV * 0.5f, 0f) * Vector3.forward * Range);

        //when the player is in range of the enemy
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(LookPosition, ChaseStoppingDistance);

        //when the player has escaped from the enemy
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(LookPosition, MaxChaseDistance);
    }


}
