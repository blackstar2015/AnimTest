using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private int _attackHash { get; set; }
    public PlayerAttackState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerController.JumpAction += Jump;
        stateMachine.PlayerController.DodgeAction += Dodge;
        stateMachine.PlayerController.BlockAction += Block;
        stateMachine.PlayerController.AttackAction += Attack;
        
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
        
        if(stateMachine.IsAttacking)
        {
            HandleAttack();
        }
        else
        {
            stateMachine.IsAttacking = false;
            stateMachine.actionIndex = 0;
            stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        }
        if(stateMachine.IsBlocking)
        {
            stateMachine.IsAttacking = false;
            stateMachine.actionIndex = 0;
            stateMachine.SwitchState(new PlayerBlockState(this.stateMachine));
        }
    }
    
    public virtual void HandleAttack()
    {
        if (!stateMachine.IsAttacking) return;
        Weapon equippedWeapon = stateMachine.Weapons[stateMachine.weaponIndex];
        WeaponMelee melee = equippedWeapon as WeaponMelee;
        if (melee == null) return; 
        if (stateMachine.actionIndex >= melee?.MeleeData.ComboData.Length) stateMachine.actionIndex = 0;
        _attackHash = melee.MeleeData.ComboData[stateMachine.actionIndex].AttackHash;
        float nextAttackTime = stateMachine.LastAttackTime + 1 / melee.MeleeData.AttackRate;
        if (Time.time < nextAttackTime) return;

        Debug.Log(stateMachine.actionIndex + " " +  _attackHash );
        stateMachine.Animator.CrossFade(_attackHash, .1f);
        melee.TryAttack(stateMachine.transform.position + stateMachine.transform.forward * 5, stateMachine.gameObject, stateMachine.Targetable.Team);

        stateMachine.actionIndex++;
        stateMachine.LastAttackTime = Time.time;
    }
}
