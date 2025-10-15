using UnityEngine;

public class PlayerLandingState : PlayerBaseState
{
    // private int _airborneFallHash => stateMachine.AirborneFallHash;
    // private int _airborneLandHash => stateMachine.AirborneLandHash;
    // private int _airborneDashHash => stateMachine.AirborneDashHash;
    
    private int _airborneFallHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneFallHash);
    private int _airborneLandHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneLandHash);
    private int _airborneDashHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneDashHash);
    public PlayerLandingState(PlayerStateMachine stateMachine, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }

    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        Debug.Log(_airborneFallHash + " " + _airborneLandHash + " " + _airborneDashHash);
        stateMachine.Animator.CrossFade(_airborneFallHash, .1f);
    }
    public override void Exit() 
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.JumpAction -= Dodge;
        base.Exit(); 
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        stateMachine.rb.AddForce(-stateMachine.transform.up * stateMachine.LandingGravity);
        if (stateMachine.IsDashing) stateMachine.SwitchState(new PlayerDodgingState(this.stateMachine, stateMachine.transform.forward, false));
        if (stateMachine.IsGrounded)
        {
            stateMachine.Animator.CrossFade(_airborneLandHash, .1f);
            stateMachine.SwitchToMovement(true);
        }
    }

    protected override void Jump()
    {
        if (stateMachine.JumpCounter < stateMachine.MaxJumps)
        {
            // calculate jump velocity from jump height and gravity
            float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight);
            // override current y velocity but maintain x/z velocity
            stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
            stateMachine.JumpCounter++;
            stateMachine.SwitchState(new PlayerAirborneState(this.stateMachine, true));
        }
    }    
}
