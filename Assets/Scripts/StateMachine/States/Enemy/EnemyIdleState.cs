using UnityEngine;

public class EnemyIdleState: EnemyBaseState
{
    private int IdleHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].IdleHash);

    public EnemyIdleState(EnemyStateMachine stateMachine, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.Animator.CrossFade(IdleHash, stateMachine.CrossFadeDuration);
        stateMachine.MoveSpeedMultiplier = stateMachine.EnemyWalkSpeedMultiplier;
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (stateMachine.EnemyController.Target != null) stateMachine.SwitchState(new EnemyChaseState(this.stateMachine));
        else stateMachine.SwitchState(new EnemyPatrolState(this.stateMachine));
    }
}
