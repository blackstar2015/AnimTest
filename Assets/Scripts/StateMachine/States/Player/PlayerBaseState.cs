using UnityEngine;

public abstract class PlayerBaseState : State
{
    protected PlayerStateMachine stateMachine;

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Tick(float deltaTime)
    {
        stateMachine.Animator.SetFloat("Speed", stateMachine.rb.linearVelocity.magnitude);
        stateMachine.Animator.SetBool("IsGrounded", stateMachine.IsGrounded);
    }
    
    public virtual void Jump()
    {
        Debug.Log("I am jumping! WEEEEEE!!!");
        if (stateMachine.IsGrounded)
        {
            stateMachine.rb.linearVelocity = new Vector3(stateMachine.rb.linearVelocity.x, stateMachine.JumpForce, stateMachine.rb.linearVelocity.z);
            stateMachine.JumpCounter++;
        }
    }
}
