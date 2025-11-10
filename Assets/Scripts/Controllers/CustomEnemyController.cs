using System.Collections;
using RPGCharacterAnims.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomEnemyController : CustomController
{
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public Targetable Target => stateMachine.CurrentTarget;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private Transform _spawnTransform;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private EnemyStateMachine stateMachine;
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] private string _currentStateName { get; set; }
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public bool CanAttackPlayer = false;

    public override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        if(_spawnTransform == null) _spawnTransform = transform;
    }
    public override void Update()
    {
        base.Update();
        _currentStateName = stateMachine.CurrentState.ToString();
        if (stateMachine.Vision.GetVisibleTargets(stateMachine.Team) != null)
        {
            stateMachine.CurrentTarget = stateMachine.Vision.GetFirstVisibleTarget(stateMachine.Team);
        }
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
}

