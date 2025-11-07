using System.Collections;
using RPGCharacterAnims.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomEnemyController : CustomController
{
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private Transform _target;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private Transform _spawnTransform;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private EnemyStateMachine stateMachine;
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] private string _currentStateName { get; set; }
    [SerializeField] private float _stopDistance = 1f;
    public bool CanAttackPlayer = false;

    public override void Awake()
    {
        base.Awake();

    }
    public override void Update()
    {
        base.Update();
        _currentStateName = stateMachine.CurrentState.ToString();
    }
    private void RemoveAttackTicket(DamageInfo damageInfo)
    {
        CanAttackPlayer = false;
    }
    public void SetStateMachine(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void SetSpawnTransform(Transform spawnTransform)
    {
        _spawnTransform = spawnTransform;
    }

    public Transform GetSpawnTransform()
    {
        return _spawnTransform;
    }
    // protected override void NotUpdate()
    // {
    //     base.Update();
    //     float distance = Vector3.Distance(transform.position, _target.position);
    //     
    //     if (distance > _stopDistance && !CanAttackPlayer)
    //     {
    //         Movement.MoveTo(_target.position);
    //     }
    //     else if (distance < _stopDistance && !CanAttackPlayer)
    //     {
    //         Movement.Stop();
    //         Movement.SetLookPosition(_target.position);
    //     }
    //     
    //     if ((_target.TryGetComponent(out Health health) && health.IsAlive))
    //     {
    //         if(CanAttackPlayer) StartCoroutine(AttackRoutine(distance));
    //     }
    //     else
    //     {
    //         CanAttackPlayer = false;
    //     }
    // }
    //
    // private IEnumerator AttackRoutine(float distance)
    // {
    //      Debug.DrawLine(transform.position + new Vector3(0,1,0), _target.position + new Vector3(0,1,0), Color.red,5f);
    //      Vector3 startPosition = transform.position;
    //      Debug.DrawLine(startPosition + new Vector3(0,1,0), _target.position + new Vector3(0,1,0), Color.blue,5f);
    //      if (distance >= Weapons[weaponIndex].Data.Range)
    //      {
    //          Movement.MoveTo(_target.position);
    //      }
    //      else
    //      {
    //          Movement.SetLookPosition(_target.position);
    //          Weapons[weaponIndex].TryAttack(_target.position, gameObject, Targetable.Team);
    //          yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length + 1);
    //          CanAttackPlayer = false;
    //          float returnDistance = Vector3.Distance(startPosition, transform.position);
    //          if(returnDistance >=.1f || !CanAttackPlayer)
    //          {
    //              Movement.MoveTo(startPosition);
    //              Movement.SetLookPosition(startPosition);
    //          }
    //          yield return new WaitUntil(() => returnDistance <= .1f);
    //      }
    //      yield return null; 
    // }

}

