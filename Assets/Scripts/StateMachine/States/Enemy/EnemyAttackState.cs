using RPGCharacterAnims.Actions;
using UnityEngine;

public class EnemyAttackState: EnemyBaseState
{
    private int _attackHash { get; set; }
    private bool _attackStarted = false;

    public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        if (stateMachine.CurrentTarget == null || !stateMachine.Vision.TestVisibility(stateMachine.CurrentTarget.transform.position)) stateMachine.SwitchState(new EnemyPatrolState(this.stateMachine));

        if (stateMachine.Vision.TestVisibility(stateMachine.CurrentTarget.transform.position) &&
            stateMachine.IsAttacking && 
            Vector3.Distance(stateMachine.transform.position, stateMachine.CurrentTarget.transform.position) <= stateMachine.ChaseStoppingDistance)
        {
            HandleAttack();
        }
        else if(!stateMachine.IsAttacking || Vector3.Distance(stateMachine.transform.position, stateMachine.CurrentTarget.transform.position) > stateMachine.ChaseStoppingDistance)
        {
            stateMachine.actionIndex = 0;
            if (!_attackStarted) stateMachine.SwitchState(new EnemyChaseState(this.stateMachine));
        }
    }


    public virtual void HandleAttack()
    {
        if (!stateMachine.IsAttacking) return;
        Weapon equippedWeapon = stateMachine.Weapons[stateMachine.weaponIndex];
        WeaponMelee melee = equippedWeapon as WeaponMelee;
        if (melee == null) return;
        if (stateMachine.actionIndex >= melee?.MeleeData.ComboData.Length) stateMachine.actionIndex = 0;
        _attackHash = Animator.StringToHash(melee.MeleeData.ComboData[stateMachine.actionIndex].AttackHashName);
        float nextAttackTime = stateMachine.LastAttackTime + 1 / melee.MeleeData.AttackRate;
        if (Time.time < nextAttackTime) return;
        _attackStarted = true;
        stateMachine.Animator.CrossFade(_attackHash, stateMachine.CrossFadeDuration);
        melee.TryAttack(stateMachine.transform.position + stateMachine.transform.forward * 5, stateMachine.gameObject, stateMachine.Targetable.Team);

        stateMachine.actionIndex++;
        stateMachine.LastAttackTime = Time.time;
        _attackStarted = false;
    }
}
