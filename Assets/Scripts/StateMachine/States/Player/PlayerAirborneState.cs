using UnityEngine;

public class PlayerAirborneState : PlayerBaseState
{
    private Vector3 _momentum;
    public PlayerAirborneState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
        base.Enter();
    }
    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        stateMachine.PlayerController.BlockAction -= Block;
        stateMachine.PlayerController.AttackAction -= Attack;
        base .Exit();
    }
    //public bool CheckWallRun()
    //{
    //    if (!stateMachine.IsGrounded) return false;

    //    bool hit = Physics.SphereCast(stateMachine.transform.position, stateMachine.WallRunCheckRadius, stateMachine.LookDirection, out RaycastHit hitInfo, stateMachine.WallRunCheckDistance, stateMachine.WallRunLayer);

    //    return hit;
    //}

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if(stateMachine.IsGrounded && stateMachine.rb.linearVelocity.magnitude <= .1f) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        if(stateMachine.IsGrounded && stateMachine.rb.linearVelocity.magnitude > .1f) stateMachine.SwitchState(new PlayerWalkingState(this.stateMachine));
        //if (CheckWallRun()) stateMachine.SwitchState(new PlayerWallRunningState(this.stateMachine));
    }
}
