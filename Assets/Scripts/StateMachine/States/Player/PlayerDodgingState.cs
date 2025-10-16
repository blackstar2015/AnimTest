using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private int _dodgeHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].DodgeHash);
    private int _airborneDashHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneDashHash);

    private Vector3 _dashDirection {  get; set; }

    public PlayerDodgingState(PlayerStateMachine stateMachine, Vector3 dashDirection, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        _dashDirection = dashDirection;
        _shouldFade = shouldFade;
    }

    public override void Enter()
    {        
        base.Enter();
        //stateMachine.Animator.applyRootMotion = true;
        stateMachine.LookInCameraDirection = false;
        stateMachine.SetLookDirection(_dashDirection);
        stateMachine.Animator.CrossFade(_dodgeHash, 0.1f);
    }
    public override void Exit()
    {
        //stateMachine.Animator.applyRootMotion = false;
        stateMachine.LookInCameraDirection = true;
        stateMachine.CanMove = true;
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        PerformDash();

        if (!stateMachine.IsDashing) stateMachine.StartCoroutine(stateMachine.SwitchToMovementWithDelay(false, 1f));
        //if(!stateMachine.IsDashing) stateMachine.SwitchToMovement(false);
    }

    private void PerformDash()
    {
        if(!stateMachine.IsDashing) return;
        stateMachine.rb.AddForce(_dashDirection * stateMachine.DashSpeed, ForceMode.Impulse);
        Debug.DrawRay(stateMachine.transform.position, _dashDirection * stateMachine.DashSpeed, Color.red, stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length);
        stateMachine.IsDashing = false;
    }

    private void EndDash()
    {
        stateMachine.SwitchToMovement(true);       
    }
}
