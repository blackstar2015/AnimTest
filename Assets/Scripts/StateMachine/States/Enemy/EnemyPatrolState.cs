using UnityEngine;

public class EnemyPatrolState: EnemyBaseState
{
    private int MoveHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].MovementHash);

    private Vector3 _patrolPoint => stateMachine.PatrolPoint;
    public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.GetPatrolPoint();
        stateMachine.Animator.CrossFade(MoveHash, stateMachine.CrossFadeDuration);
    }
    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (stateMachine.EnemyController.Target != null) stateMachine.SwitchState(new EnemyChaseState(this.stateMachine));
        StartPatrol();
    }

    private void StartPatrol()
    {
        stateMachine.MoveTo(_patrolPoint);
        if (Vector3.Distance(stateMachine.transform.position, _patrolPoint) <= stateMachine.StoppingDistance)
        {
            stateMachine.Stop();
            Debug.Log(Vector3.Distance(stateMachine.transform.position, _patrolPoint));
            stateMachine.GetPatrolPoint();
        }
    }
}
