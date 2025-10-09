using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private int _currentActionIndex;
    public PlayerAttackState(PlayerStateMachine stateMachine, int currentActionIndex) : base(stateMachine)
    {
        _currentActionIndex = currentActionIndex;
    }
    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
        string attack = "Attack" + (_currentActionIndex+1).ToString();
        Debug.Log("Attack" + _currentActionIndex.ToString());
        int AttackHash = Animator.StringToHash(attack); 
        stateMachine.Animator.CrossFade(AttackHash, .1f);
    }
    public override void Exit()
    {
        stateMachine.PlayerController.JumpAction -= Jump;
        stateMachine.PlayerController.DodgeAction -= Dodge;
        stateMachine.PlayerController.BlockAction -= Block;
        stateMachine.PlayerController.AttackAction -= Attack;
        base .Exit();
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        
        stateMachine.HandleAttack();
        if(!stateMachine.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        }
        else
        {
            stateMachine.Invoke(nameof(stateMachine.ContinueAttack),stateMachine.Animator.GetCurrentAnimatorStateInfo(1).length);
        }
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine));
    }
}
