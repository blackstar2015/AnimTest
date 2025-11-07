using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerBaseState : State
{
    public PlayerStateMachine stateMachine;
    protected bool _shouldFade;
    protected float _crossfadeDuration = .1f;
    public PlayerBaseState(PlayerStateMachine stateMachine, bool shouldFade = true)
    {
        this.stateMachine = stateMachine;
        _shouldFade = shouldFade;
    }

    public override void Enter()
    {
        if(stateMachine.debugStateTransitions) Debug.Log("Entering " + stateMachine.CurrentState);
        if(_shouldFade) stateMachine.CrossFadeDuration = _crossfadeDuration;
        else stateMachine.CrossFadeDuration = 0f;
    }
    public override void Exit()
    {
        if (stateMachine.debugStateTransitions) Debug.Log("Exiting " + stateMachine.CurrentState);
    }
    public override void Tick(float deltaTime)
    {
        if (stateMachine.debugStateTransitions) Debug.Log("Current State " + stateMachine.CurrentState);
    }

    protected virtual void Jump()
    {
        if(stateMachine.JumpCounter < stateMachine.MaxJumps)
        {
            // calculate jump velocity from jump height and gravity
            float jumpVelocity = Mathf.Sqrt(2f * -stateMachine.Gravity * stateMachine.JumpHeight);
            // override current y velocity but maintain x/z velocity
            stateMachine.Velocity = new Vector3(stateMachine.Velocity.x, jumpVelocity, stateMachine.Velocity.z);
            stateMachine.JumpCounter++;
        }
    }
    protected virtual void TryJump()
    {
        if (!machine.CanMove || !machine.CanCoyoteJump) return;
        Jump();
    }

    protected  void Dodge()
    {
        stateMachine.IsDashing = true;
    }
    
    protected void Attack(bool isPressed)
    {
        stateMachine.IsAttacking = isPressed;
    }

    protected void Block(bool isPressed)
    {
        stateMachine.isBlocking = stateMachine.CanBlock && isPressed;
    }

    protected void Sprint(bool isPressed)
    {
        stateMachine.MoveSpeedMultiplier = isPressed ? 2 : 1;
    }

    protected void WeaponSwitch()
    {
        int switchFromHash = Animator.StringToHash("SwitchingFrom");
        stateMachine.Animator.CrossFade(switchFromHash, 0.1f);
        if (stateMachine.CurrentWeaponIndex >= stateMachine.Weapons.Length - 1)
        {
            stateMachine.weaponIndex = 0;
        }
        else
        {
            stateMachine.weaponIndex++;
        }
        foreach (Weapon weapon in stateMachine.Weapons)
        {
            if (weapon.Data.WeaponIndex == stateMachine.weaponIndex)
            {
                weapon.WeaponMesh.SetActive(true);
            }
            else
            {
                weapon.WeaponMesh.SetActive(false);
            }
        }
        int switchToHash = Animator.StringToHash("SwitchingTo");
        stateMachine.Animator.CrossFade(switchToHash, 0.2f);
    }


    protected virtual void TargetLock()
    {
        if(stateMachine.CurrentTarget == null)
        {
            List<Targetable> possibleTargets = new List<Targetable>();
            possibleTargets =  stateMachine.Vision.GetVisibleTargets(0);
            stateMachine.CurrentTarget = possibleTargets[stateMachine.Vision.CurrentVisibleIndex];
            if(stateMachine.CurrentTarget != null)
            {
                stateMachine.IsTargeting = true;
                stateMachine.PlayerController.TargetLockCam.Priority = 2;
                stateMachine.PlayerController.TargetGroup.Targets[1].Object = stateMachine.CurrentTarget.transform;
                //stateMachine.PlayerController.TargetGroup.Targets[1].Radius = 1;
                //stateMachine.PlayerController.TargetGroup.Targets[1].Weight = .1f;
            }
        }
        else
        {
            stateMachine.PlayerController.TargetLockCam.Priority = 0;
            stateMachine.IsTargeting = false;
            stateMachine.CurrentTarget = null;
            //stateMachine.PlayerController.TargetGroup.Targets[1] = null;
        }
    }

}
