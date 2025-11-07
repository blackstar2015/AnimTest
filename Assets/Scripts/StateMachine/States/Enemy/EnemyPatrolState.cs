using UnityEngine;

public class EnemyPatrolState: EnemyBaseState
{
    private Vector3 _patrolPoint => stateMachine.PatrolPoint;
    public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.GetPatrolPoint();
    }
    public override void Exit()
    {
        base.Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        
        stateMachine.MoveTo(_patrolPoint);
        if (Vector3.Distance(stateMachine.transform.position, _patrolPoint) <= stateMachine.StoppingDistance)
        {
            stateMachine.Stop();
            Debug.Log(Vector3.Distance(stateMachine.transform.position, _patrolPoint));
            stateMachine.GetPatrolPoint();
        }
    }

}
