using UnityEngine;

public class PlayerBlockState: PlayerBaseState
{
    private int _blockHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].BlockHash);
    public PlayerBlockState(PlayerStateMachine stateMachine, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.Animator.CrossFade(_blockHash, stateMachine.CrossFadeDuration);
    }
    public override void Exit()
    {
        stateMachine.PlayerController.BlockAction -= Block;
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(!stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine, true));
    }
}
