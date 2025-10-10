using UnityEngine;

public class PlayerBlockState: PlayerBaseState
{
    private int _blockHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].BlockHash);
    public PlayerBlockState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.Animator.CrossFade(_blockHash, 0.1f);
    }
    public override void Exit()
    {
        stateMachine.PlayerController.BlockAction -= Block;
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(!stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
    }
}
