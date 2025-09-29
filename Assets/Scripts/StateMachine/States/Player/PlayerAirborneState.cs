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
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override  void Jump()
    {
        
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
        if(stateMachine.IsGrounded) stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        //if (CheckWallRun()) stateMachine.SwitchState(new PlayerWallRunningState(this.stateMachine));
    }
}
