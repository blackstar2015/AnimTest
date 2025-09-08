using UnityEngine;

public class CustomController : MonoBehaviour
{
    [field: SerializeField] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField] protected Animator Animator { get; set; }
    public Health Health { get; private set; }
    public Targetable Targetable { get; private set; }
    public Vision Vision { get; private set; }
    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
    }
    
    protected virtual void Awake()
    {
        //Cursor.lockState = CursorMode;
        Movement = GetComponent<CustomCharacterMovement>();
        Health = GetComponent<Health>();
        Targetable = GetComponent<Targetable>();
        Vision = GetComponent<Vision>();
    }
}
