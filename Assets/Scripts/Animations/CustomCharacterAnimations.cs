using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class CustomCharacterAnimations : MonoBehaviour
{
    // damping time smooths rapidly changing values sent to animator
    [field: SerializeField, TabGroup("Components"), ReadOnly] private StateMachine _stateMachine;
    [field: SerializeField, TabGroup("Components"), ReadOnly] protected Animator Animator { get; set; }
    [field: SerializeField, TabGroup("Parameters"), ReadOnly] private float _speed { get; set; }
    [field: SerializeField, TabGroup("Parameters"), ReadOnly] private bool _isMoving { get; set; }
    [field: SerializeField, TabGroup("Parameters"), ReadOnly] private float _velocityX { get; set; }
    [field: SerializeField, TabGroup("Parameters"), ReadOnly] private float _velocityY { get; set; }
    [field: SerializeField, TabGroup("Parameters"), ReadOnly] private float _velocityZ { get; set; }
    [field: SerializeField, TabGroup("Properties"), ReadOnly] protected float DampTime { get; set; } = 0.1f;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isGrounded =>_stateMachine.IsGrounded;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isAlive =>_stateMachine.IsAlive;        
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isHitReacting => _stateMachine.IsHitReacting;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isBlocking => _stateMachine.IsBlocking;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _canBlock => _stateMachine.CanBlock;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isBlockedAttack => _stateMachine.IsBlockedAttack;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private bool _isDashing => _stateMachine.IsDashing;
    [ShowInInspector, TabGroup("Properties"), ReadOnly]private int _jumpCounter => _stateMachine.JumpCounter;

    protected virtual void OnValidate()
    {
        if (Animator == null) Animator = GetComponent<Animator>();
        if(_stateMachine == null) _stateMachine = GetComponent<StateMachine>();
    }

    protected virtual void Update()
    {
        Vector3 velocity = _stateMachine.Velocity;
        _velocityY =  velocity.y;
        Vector3 flattenedVelocity = new Vector3(velocity.x, 0f, velocity.z);
        _speed = Mathf.Min(_stateMachine.LocalMoveInput.magnitude, flattenedVelocity.magnitude / _stateMachine.Speed);
        _isMoving = _speed > 0 ? true : false;
        velocity = transform.InverseTransformDirection(velocity);
        _velocityX =  velocity.x * Mathf.Abs(_stateMachine.LocalMoveInput.x);
        _velocityZ =  velocity.z *  Mathf.Abs(_stateMachine.LocalMoveInput.z);
        Animator.SetFloat("Speed", _speed, DampTime, Time.deltaTime);
        Animator.SetBool("Moving",_isMoving);
        Animator.SetBool("IsGrounded", _isGrounded);
        Animator.SetFloat("VerticalVelocity", _velocityY);
        Animator.SetFloat("VelocityX", _velocityX);
        Animator.SetFloat("VelocityZ", _velocityZ);
        Animator.SetInteger("JumpCounter", _jumpCounter);
        Animator.SetBool("IsAlive", _isAlive);
        Animator.SetBool("HitReact", _isHitReacting);
        Animator.SetBool("IsBlocking", _isBlocking);
        Animator.SetBool("CanBlock", _canBlock);
        Animator.SetBool("BlockedAttack", _isBlockedAttack);
        Animator.SetBool("Dashing",_isDashing);


    }
    #region AnimationEvents
    public void Sheath(int index)
    {
        GameObject weaponMesh = _stateMachine.Weapons[index].WeaponMesh;
        weaponMesh.SetActive(false);
    }

    public void UnSheath(int index)
    {
        GameObject weaponMesh = _stateMachine.Weapons[index].WeaponMesh;
        weaponMesh.SetActive(true);
    }

    public void DisableTrigger(int index)
    {
        foreach (Collider collider in _stateMachine.Weapons[index].WeaponColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableTrigger(int index)
    {
        foreach (Collider collider in _stateMachine.Weapons[index].WeaponColliders)
        {
            Debug.Log(collider.gameObject, collider.gameObject);
            collider.enabled = true;
        }
    }
    #endregion
}
