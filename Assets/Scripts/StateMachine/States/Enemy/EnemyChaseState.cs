using UnityEngine;

public class EnemyChaseState: EnemyBaseState
{
    private int MoveHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].MovementHash);

    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.Animator.CrossFade(MoveHash, stateMachine.CrossFadeDuration);
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if(Vector3.Distance(stateMachine.transform.position, stateMachine.EnemyController.Target.position) >= stateMachine.StoppingDistance)
        {
            stateMachine.MoveTo(stateMachine.EnemyController.Target.position);
        }
    }
}
