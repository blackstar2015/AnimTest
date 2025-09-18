using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class CustomCharacterAnimations : MonoBehaviour
{
    // damping time smooths rapidly changing values sent to animator
    [field: SerializeField] protected float DampTime { get; set; } = 0.1f;

    [field: SerializeField, TabGroup("Components")] protected Animator Animator { get; set; }
    [field: SerializeField, TabGroup("Components")] protected CustomController _controller { get; set; }
    [field: SerializeField, TabGroup("Components")] protected CustomCharacterMovementBase CharacterMovement { get; set; }

    [TabGroup("Events")]public UnityEvent OnFootR = new UnityEvent();
    [TabGroup("Events")]public UnityEvent OnFootL = new UnityEvent();
    [TabGroup("Events")]public UnityEvent OnLand = new UnityEvent();
    [TabGroup("Events")]public UnityEvent OnShoot = new UnityEvent();
    [TabGroup("Events")]public UnityEvent OnHit = new UnityEvent();
    
    
    protected virtual void OnValidate()
    {
        if (Animator == null) Animator = GetComponent<Animator>();
        if (CharacterMovement == null) CharacterMovement = GetComponent<CustomCharacterMovementBase>();
        if(_controller == null) _controller = GetComponent<CustomController>();
    }

    protected virtual void Update()
    {
        Vector3 velocity = CharacterMovement.Velocity;
        Vector3 flattenedVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float speed = Mathf.Min(CharacterMovement.MoveInput.magnitude, flattenedVelocity.magnitude / CharacterMovement.Speed);
        bool isMoving = speed > 0 ? true : false;
        velocity = transform.InverseTransformDirection(velocity);
        float velocityX =  velocity.x * Mathf.Abs(CharacterMovement.MoveInput.x);
        float velocityZ =  velocity.z *  Mathf.Abs(CharacterMovement.MoveInput.z);
        
        Animator.SetFloat("Speed", speed, DampTime, Time.deltaTime);
        Animator.SetBool("Moving",isMoving);
        Animator.SetBool("IsGrounded", CharacterMovement.IsGrounded);
        Animator.SetFloat("VerticalVelocity", velocity.y);
        Animator.SetFloat("VelocityX", velocityX);
        Animator.SetFloat("VelocityZ", velocityZ);
        Animator.SetBool("IsAlive", _controller.IsAlive);
        Animator.SetBool("HitReact", _controller.IsHitReacting);
        Animator.SetBool("IsBlocking", _controller.IsBlocking);
        Animator.SetBool("CanBlock", _controller.CanBlock);
        Animator.SetBool("BlockedAttack", _controller.IsBlockedAttack);
        
    }
    #region AnimationEvents
    public void Sheath(int index)
    {
        GameObject weaponMesh = _controller.Weapons[index].WeaponMesh;
        weaponMesh.SetActive(false);
    }

    public void UnSheath(int index)
    {
        GameObject weaponMesh = _controller.Weapons[index].WeaponMesh;
        weaponMesh.SetActive(true);
    }

    public void DisableTrigger(int index)
    {
        foreach (Collider collider in _controller.Weapons[index].WeaponColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableTrigger(int index)
    {
        foreach (Collider collider in _controller.Weapons[index].WeaponColliders)
        {
            Debug.Log(collider.gameObject, collider.gameObject);
            collider.enabled = true;
        }
    }
    #endregion
    public void FootR() => OnFootR.Invoke();
    public void FootL() => OnFootL.Invoke();
    public void Land() => OnLand.Invoke();
    public void Hit() => OnHit.Invoke();
    public void Shoot() => OnShoot.Invoke();
}
