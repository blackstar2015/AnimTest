using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    public PlayerStateMachine stateMachine;
    private Vector3 _dashDirection;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void Exit()
    {

    }
    public override void Tick(float deltaTime)
    {
        
    }

    protected override void Jump()
    {
        // calculate jump velocity from jump height and gravity
        float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight);
        // override current y velocity but maintain x/z velocity
        stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
    }

    protected override void Dodge()
    {
        if (!stateMachine.CanMove) return;
        
        //StartCoroutine(DashCoroutine(DashAnimLength));
    }
    private IEnumerator DashCoroutine(float DashAnimLength)
    {
        if (!stateMachine.CanMove) yield break;
        stateMachine.IsDashing = true;
        if (stateMachine.LocalMoveInput == Vector3.zero) _dashDirection = -1 * stateMachine.transform.forward;
        else _dashDirection = stateMachine.LocalMoveInput.normalized;
        stateMachine.SetLookDirection(_dashDirection);
        stateMachine.rb.AddForce(_dashDirection * stateMachine.DashSpeed);

        yield return new WaitForSeconds(DashAnimLength);

        stateMachine.IsDashing = false;
        yield return null;
    }
    protected override void Attack()
    {

    }

    protected override void Block()
    {
        
    }
}
