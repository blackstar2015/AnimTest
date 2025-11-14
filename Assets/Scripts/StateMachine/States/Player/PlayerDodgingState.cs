using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDodgingState : PlayerBaseState
{
    private int _dodgeHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].DodgeHash);
    private int _lockedDodgeHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].LockedDodgeHash);
    private int _airDodgeHash => Animator.StringToHash(stateMachine.Weapons[stateMachine.CurrentWeaponIndex].AirborneDashHash);
    private Vector3 _dashDirection {  get; set; }
    private Vector3 _moveInput { get; set; }

    public PlayerDodgingState(PlayerStateMachine stateMachine, Vector3 dashDirection, bool shouldFade) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
        if(stateMachine.IsTargeting)
        {
            _dashDirection = (stateMachine.CurrentTarget.transform.position - stateMachine.transform.position).normalized;
        }
        else _dashDirection = dashDirection.normalized;
        _shouldFade = shouldFade;
        _moveInput = -stateMachine.LocalMoveInput;
    }

    public override void Enter()
    {        
        base.Enter();
        
        if (stateMachine.IsTargeting || stateMachine.CurrentTarget != null)
        {
            stateMachine.SetLookPosition(stateMachine.CurrentTarget.transform.position);
        }
        else if(!stateMachine.IsTargeting)
        {
            stateMachine.LookInCameraDirection = false;
            stateMachine.SetLookDirection(_dashDirection);
        }
        stateMachine.PlayerController.DodgeAction += Dodge;
    }
    public override void Exit()
    {
        stateMachine.LookInCameraDirection = true;
        stateMachine.CanMove = true;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        if (stateMachine.IsTargeting || stateMachine.CurrentTarget != null)
        {
            stateMachine.SetLookPosition(stateMachine.CurrentTarget.transform.position);
        }
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (!stateMachine.IsDashing && ! stateMachine.IsTargeting) stateMachine.StartCoroutine(stateMachine.SwitchToMovementWithDelay(false, stateMachine.Animator.GetCurrentAnimatorStateInfo(0).length - .5f));
        else if (!stateMachine.IsDashing && stateMachine.IsTargeting) stateMachine.StartCoroutine(stateMachine.SwitchToMovementWithDelay(true, .1f));

        PerformDash();
        //if (stateMachine.IsGrounded)
        //{
        //}
        //else PerformAirDash();
    }

    private void PerformDash()
    {
        if(!stateMachine.IsDashing) return;
        if (!stateMachine.IsTargeting) stateMachine.StartCoroutine(DashCoroutine());
        else stateMachine.StartCoroutine(TargetLockDashCoroutine());
        
    }

    private void PerformAirDash()
    {
        stateMachine.StartCoroutine(AirDashCoroutine());
    }

    private IEnumerator AirDashCoroutine()
    {
        stateMachine.IsDashing = true;
        stateMachine.rb.linearVelocity = Vector3.zero;
        yield return new WaitForEndOfFrame();
        stateMachine.rb.AddForce(stateMachine.transform.forward *  stateMachine.DashDistance,ForceMode.Acceleration);
        yield return new WaitForSeconds(1);
        stateMachine.IsDashing = false;
    }

    private IEnumerator TargetLockDashCoroutine()
    {
        if (!stateMachine.IsGrounded) yield return null;
        float direction = _moveInput.x >= 0 ? 1f : -1f;
        stateMachine.IsDashing = true;
        stateMachine.Animator.CrossFade(_lockedDodgeHash, stateMachine.CrossFadeDuration);

        float elapsedTime = 0f;

        Transform player = stateMachine.transform;
        float dashDuration = stateMachine.DashDuration/2;


        if (stateMachine.IsTargeting && stateMachine.CurrentTarget != null)
        {
            stateMachine.SetLookPosition(stateMachine.CurrentTarget.transform.position);
            Transform target = stateMachine.CurrentTarget.transform;
            Vector3 toPlayer = player.position - target.position;
            float radius = toPlayer.magnitude;

            float dashDistance = stateMachine.DashDistance / 2 * MathF.PI * radius;
            float dashSpeed = dashDistance / dashDuration;
            
            float totalDegrees = stateMachine.LockDashArc * direction; 

            while (elapsedTime < dashDuration)
            {
                float t = elapsedTime / dashDuration;
                float angle = totalDegrees * t;

                Vector3 rotated = Quaternion.AngleAxis(angle, Vector3.up) * toPlayer;

                Vector3 newPos = target.position + rotated.normalized * radius;

                player.position = Vector3.Lerp(player.position, newPos, Time.deltaTime * dashSpeed);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        stateMachine.IsDashing = false;
    }

    private IEnumerator DashCoroutine()
    {
        if (!stateMachine.IsGrounded) yield return null;
        stateMachine.IsDashing = true;
        Vector3 right = Camera.main.transform.right;
        Vector3 dashForward = Vector3.Cross(-stateMachine.GroundNormal, right).normalized;
        Vector3 startPos = stateMachine.transform.position;
        Vector3 endPos = stateMachine.transform.position + _dashDirection * stateMachine.DashDistance;
        Debug.DrawLine(startPos, endPos, Color.red, 10f);
        float elapsedTime = 0f;
        float originalGravity = stateMachine.LandingGravity;

        if (!stateMachine.IsGrounded)
        {
            stateMachine.LandingGravity = 0;
            stateMachine.DashDuration = .1f;
        }
        while (elapsedTime < stateMachine.DashDuration)
        {
            if (stateMachine.IsGrounded) stateMachine.Animator.CrossFade(_dodgeHash, stateMachine.CrossFadeDuration);

            float t = elapsedTime / (stateMachine.DashDuration);
            stateMachine.transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        stateMachine.LandingGravity = originalGravity;
        stateMachine.IsDashing = false;
    }
}
