using System.Collections;
using CharacterMovement;
using UnityEngine;

public class CustomCharacterMovement : CharacterMovement3D
{
    private Vector3 _dashDirection;
    public bool IsDashing = false;
    public float DashCooldown = 2f;
    [SerializeField] private float _dashSpeed = 1000f;

    public void Dash(float DashAnimLength)
    {
        StartCoroutine(DashCoroutine(DashAnimLength));
        
    }

    private IEnumerator DashCoroutine(float DashAnimLength)
    {
        IsDashing = true;
        if(LocalMoveInput == Vector3.zero) _dashDirection = -1 * transform.forward;
        else _dashDirection = LocalMoveInput.normalized;
        SetLookDirection(_dashDirection);
        Rigidbody.AddForce(_dashDirection * _dashSpeed );
        
        yield return new WaitForSeconds(1);
        
        Debug.Log(DashAnimLength);
        IsDashing = false;
        yield return null;
    }
}
