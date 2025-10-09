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
        string attack = "Attack" + (_currentActionIndex).ToString();
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
        
        if(!stateMachine.IsAttacking)
        {
            stateMachine.SwitchState(new PlayerIdleState(this.stateMachine));
        }
        else
        {
            stateMachine.Invoke(nameof(stateMachine.ContinueAttack),5);
        }
        if(stateMachine.IsBlocking) stateMachine.SwitchState(new PlayerBlockState(this.stateMachine));
    }
    
    public virtual void HandleAttack()
    {
        if (!stateMachine.IsAttacking) return;
        Weapon equippedWeapon = stateMachine.Weapons[stateMachine.weaponIndex];
        float nextAttackTime = stateMachine.LastAttackTime + 1 / equippedWeapon.Data.AttackRate;

        if (Time.time < nextAttackTime) return;

        equippedWeapon.TryAttack(stateMachine.transform.position + stateMachine.transform.forward * 5, stateMachine.gameObject, stateMachine.Targetable.Team);
        WeaponMelee melee = equippedWeapon as WeaponMelee;
        if (melee == null) return;
        stateMachine.actionIndex++;
        if (stateMachine.actionIndex > melee?.MeleeData.ComboData.Length - 1) stateMachine.actionIndex = 1;
        stateMachine.LastAttackTime = Time.time;
    }
}
