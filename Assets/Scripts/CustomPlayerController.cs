using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerController : MonoBehaviour
{
    // initial cursor state
    [field: SerializeField] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [field: SerializeField] public bool LookInCameraDirection { get; set; }

    [field: Header("Componenents")]
    [field: SerializeField] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField] protected Animator Animator { get; set; }

    [field: SerializeField] protected int ActionList = 6;
    private int _actionIndex = 1;

    // public Health Health { get; private set; }
    // public Targetable Targetable { get; private set; }
    // public Vision Vision { get; private set; }

    public bool CanShoot { get; set; } = true;
    public bool CanMelee { get; set; } = true;

    // array of current weapons
    // InlineButton appears beside the property/field in the inspector
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find")] public Weapons[] Weapons { get; private set; }
    protected Vector2 MoveInput { get; set; }

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
    }

    protected virtual void Awake()
    {
        Cursor.lockState = CursorMode;
        Movement = GetComponent<CustomCharacterMovement>();
        // Health = GetComponent<Health>();
        // Targetable = GetComponent<Targetable>();
        // Vision = GetComponent<Vision>();
    }

    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapons>();
    }
    public virtual void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public virtual void OnJump(InputValue value)
    {
        Movement?.TryJump();
    }

    public virtual void OnDash(InputValue value)
    {
        Movement?.Dash(Animator.GetCurrentAnimatorStateInfo(0).length);
        Animator?.SetTrigger("Dash");
    }
    public virtual void OnAttack(InputValue value)
    {
        Debug.Log("asd");
        Animator.SetTrigger("Trigger");
        Animator.SetInteger("Action", _actionIndex);
        _actionIndex++;
        if (_actionIndex > ActionList)
        {
            _actionIndex = 1;
        }
        //Animator.ResetTrigger("Trigger");
    }

    protected virtual void Update()
    {
        if (Movement == null) return;

        // find correct right/forward directions based on main camera rotation
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = forward * MoveInput.y + right * MoveInput.x;

        // send player input to character movement
        Movement.SetMoveInput(moveInput);
        Movement.SetLookDirection(moveInput);
        LookInCameraDirection = !Movement.IsDashing;
        if (LookInCameraDirection) Movement.SetLookDirection(Camera.main.transform.forward);
    }
}
