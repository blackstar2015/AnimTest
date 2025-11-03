//using UnityEngine;

//public class PlayerLockTargetState: PlayerBaseState
//{
//    public PlayerLockTargetState(PlayerStateMachine stateMachine) : base(stateMachine)
//    {
//        this.stateMachine = stateMachine;
//    }
    
//    public override void Enter()
//    {
//        base.Enter();
//        stateMachine.PlayerController.JumpAction += Jump;
//        stateMachine.PlayerController.DodgeAction += Dodge;
//        stateMachine.PlayerController.BlockAction += Block;
//        stateMachine.PlayerController.AttackAction += Attack;
//        stateMachine.PlayerController.SprintAction += Sprint;
//        stateMachine.PlayerController.WeaponSwitchAction += WeaponSwitch;
//        stateMachine.PlayerController.TargetLockAction += TargetLock;
//        stateMachine.LookInCameraDirection = false;

//    }
//    public override void Exit()
//    {
//        base.Exit();
//        stateMachine.PlayerController.JumpAction -= Jump;
//        stateMachine.PlayerController.DodgeAction -= Dodge;
//        stateMachine.PlayerController.BlockAction -= Block;
//        stateMachine.PlayerController.AttackAction -= Attack;
//        stateMachine.PlayerController.SprintAction -= Sprint;
//        stateMachine.PlayerController.WeaponSwitchAction -= WeaponSwitch;
//        stateMachine.PlayerController.TargetLockAction -= TargetLock;
//        stateMachine.LookInCameraDirection = true;
//        stateMachine.CurrentTarget = null;
//    }
//    public override void Tick(float deltaTime)
//    {
//        base.Tick(deltaTime);
//        if (stateMachine.CurrentTarget != null)
//        {
//            stateMachine.SetLookPosition(stateMachine.CurrentTarget.transform.position);
//        }
//    }

//    protected override void TargetLock()
//    {
//        stateMachine.SwitchState(new PlayerIdleState(this.stateMachine,false));
//    }
//}
