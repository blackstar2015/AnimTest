using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public abstract class State 
{
    protected StateMachine machine;
    public abstract void Enter();
    public abstract void Exit();
    public abstract void Tick(float deltaTime);

    protected float GetNormalizedTime(Animator animator, string tag)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        else return 0f;
    }
    protected virtual void SetMoveInput(Vector3 input)
    {
        if (!machine.CanMove)
        {
            machine.MoveInput = Vector3.zero;
            return;
        }
        input = Vector3.ClampMagnitude(input, 1f);
        // set input to 0 if small incoming value
        machine.HasMoveInput = input.magnitude > 0.1f;
        input = machine.HasMoveInput ? input : Vector3.zero;
        // remove y component of movement but retain overall magnitude
        Vector3 flattened = new Vector3(input.x, 0f, input.z);
        flattened = flattened.normalized * input.magnitude;
        machine.MoveInput = flattened;
        // finds movement input as local direction rather than world direction
        machine.LocalMoveInput = machine.transform.InverseTransformDirection(machine.MoveInput);
    }
    protected virtual void TryJump()
    {
        if (!machine.CanMove || !machine.CanCoyoteJump) return;
        Jump();
    }
    protected abstract void Jump();
    protected abstract void Dodge();
    protected abstract void WeaponSwitch();
    protected abstract void Attack(bool isPressed);
    protected abstract void Block(bool isPressed);
    protected abstract void Sprint(bool isPressed);
    protected virtual void MoveTo(Vector3 destination)
    {
        if (!machine.NavAgent.isActiveAndEnabled || !machine.NavAgent.isOnNavMesh) return;
        machine.NavAgent.SetDestination(destination);
    }
    protected virtual void Stop()
    {
        SetMoveInput(Vector3.zero);
        if (!machine.NavAgent.isActiveAndEnabled || !machine.NavAgent.isOnNavMesh) return;
        machine.NavAgent.ResetPath();
    }
    protected virtual bool CheckGrounded()
    {
        // raycast to find ground
        bool hit = Physics.Raycast(machine.GroundCheckStart, -machine.transform.up, out RaycastHit hitInfo, machine.GroundCheckDistance, machine.GroundMask);

        // set default ground surface normal and SurfaceVelocity
        machine.GroundNormal = Vector3.up;
        machine.SurfaceVelocity = Vector3.zero;

        // if ground wasn't hit, character is not grounded
        if (!hit) return false;

        // gets velocity of surface underneath character if applicable
#if UNITY_6000_0_OR_NEWER
        if (hitInfo.rigidbody != null) machine.SurfaceVelocity = hitInfo.rigidbody.linearVelocity;
#else
            if (hitInfo.rigidbody != null) SurfaceVelocity = hitInfo.rigidbody.velocity;
#endif

        // test angle between character up and ground, angles above _maxSlopeAngle are invalid
        bool angleValid = Vector3.Angle(machine.transform.up, hitInfo.normal) < machine.MaxSlopeAngle;
        if (angleValid)
        {
            // record last time character was grounded and set correct floor normal direction
            machine.LastGroundedTime = Time.timeSinceLevelLoad;
            machine.GroundNormal = hitInfo.normal;
            machine.LastGroundedPosition = machine.transform.position;
            machine.SurfaceObject = hitInfo.collider.gameObject;
            if (machine.ParentToSurface) machine.transform.SetParent(machine.SurfaceObject.transform);
            return true;
        }

        machine.SurfaceObject = null;
        if (machine.ParentToSurface) machine.transform.SetParent(null);
        return false;
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        float landingCollisionMaxDistance = 0.25f;
        Vector3 point = collision.contacts[0].point;
        if (Mathf.Abs(collision.relativeVelocity.y) < machine.MinGroundedVelocity) return;
        if (Vector3.Distance(point, machine.transform.position) < landingCollisionMaxDistance)
        {
            machine.OnGrounded.Invoke(collision.gameObject);
        }
    }
}
