using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private int _dodgeHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].DodgeHash);
    private int _airborneDashHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneDashHash);

    private Vector3 _dashDirection {  get; set; }

    public PlayerDodgingState(PlayerStateMachine stateMachine, Vector3 dashDirection, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _dashDirection = dashDirection.normalized;
        _shouldFade = shouldFade;
    }

    public override void Enter()
    {        
        base.Enter();
        
        stateMachine.LookInCameraDirection = false;
        stateMachine.SetLookDirection(_dashDirection);
        stateMachine.Animator.CrossFade(_dodgeHash, stateMachine.CrossFadeDuration);
        stateMachine.PlayerController.DodgeAction += Dodge;
    }
    public override void Exit()
    {
        stateMachine.LookInCameraDirection = true;
        stateMachine.CanMove = true;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (!stateMachine.IsDashing) stateMachine.StartCoroutine(stateMachine.SwitchToMovementWithDelay(false, stateMachine.Animator.GetCurrentAnimatorStateInfo(0).length));
        PerformDash();
    }

    private void PerformDash()
    {
        if(!stateMachine.IsDashing) return;
        stateMachine.rb.linearVelocity = Vector3.zero;
        stateMachine.CanMove = false;
        //stateMachine.rb.AddForce(_dashDirection * stateMachine.DashSpeed, ForceMode.Impulse);
        Debug.DrawRay(stateMachine.transform.position + Vector3.up, _dashDirection * stateMachine.DashSpeed, Color.red, 5);
        stateMachine.IsDashing = false;
    }

    private void EndDash()
    {
        stateMachine.SwitchToMovement(true);
    }
}
