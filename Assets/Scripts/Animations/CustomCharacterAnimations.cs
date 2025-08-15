using CharacterMovement;
using UnityEngine;

public class CustomCharacterAnimations : MonoBehaviour
{
    // damping time smooths rapidly changing values sent to animator
    [field: SerializeField] protected float DampTime { get; set; } = 0.1f;

    [field: Header("Components")]
    [field: SerializeField] protected Animator Animator { get; set; }
    [field: SerializeField] protected CharacterMovementBase CharacterMovement { get; set; }

    protected virtual void OnValidate()
    {
        if (Animator == null) Animator = GetComponent<Animator>();
        if (CharacterMovement == null) CharacterMovement = GetComponent<CharacterMovementBase>();
    }

    protected virtual void Update()
    {
        // send velocity to animator, ignoring y-velocity
        Vector3 velocity = CharacterMovement.Velocity;
        Vector3 flattenedVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float speed = Mathf.Min(CharacterMovement.MoveInput.magnitude, flattenedVelocity.magnitude / CharacterMovement.Speed);
        Animator.SetFloat("Speed", speed, DampTime, Time.deltaTime);
        // send grounded state
        Animator.SetBool("IsGrounded", CharacterMovement.IsGrounded);
        // send isolated y-velocity
        Animator.SetFloat("VerticalVelocity", velocity.y);
        bool isMoving = speed > 0 ? true : false;
        Animator.SetBool("Moving",isMoving);
        velocity = transform.InverseTransformDirection(velocity);
        float velocityX =  velocity.x * Mathf.Abs(CharacterMovement.MoveInput.x);
        float velocityZ =  velocity.z *  Mathf.Abs(CharacterMovement.MoveInput.z);
        Animator.SetFloat("VelocityX", velocityX);
        Animator.SetFloat("VelocityZ", velocityZ);
    }
}
