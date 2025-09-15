using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CustomCharacterMovement : CustomCharacterMovementBase
{
    [SerializeField, TabGroup("Dashing")] private float _dashSpeed = 1000f;
    [ShowInInspector, TabGroup("Dashing")]public bool IsDashing { get; private set; } = false;
    [ShowInInspector, TabGroup("Dashing")]public float DashCooldown { get; private set; } = 2f;
    private Vector3 _dashDirection;

    public void Dash(float DashAnimLength)
    {
        if (!CanMove) return;
        StartCoroutine(DashCoroutine(DashAnimLength));
    }

    private IEnumerator DashCoroutine(float DashAnimLength)
    {
        if (!CanMove) yield break;
        IsDashing = true;
        if(LocalMoveInput == Vector3.zero) _dashDirection = -1 * transform.forward;
        else _dashDirection = LocalMoveInput.normalized;
        SetLookDirection(_dashDirection);
        Rigidbody.AddForce(_dashDirection * _dashSpeed );
        
        yield return new WaitForSeconds(DashAnimLength);
        
        IsDashing = false;
        yield return null;
    }
}
