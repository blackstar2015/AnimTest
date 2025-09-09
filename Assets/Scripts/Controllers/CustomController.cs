using Sirenix.OdinInspector;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    [field: SerializeField] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField] protected Animator Animator { get; set; }
    public Health Health { get; private set; }
    public Targetable Targetable { get; private set; }
    public Vision Vision { get; private set; }
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find")] public Weapons[] Weapons { get; private set; }
    public int CurrentWeaponIndex => _weaponIndex;
    public int CurrentActionIndex => _actionIndex;
    protected int _actionIndex = 1;
    protected int _weaponIndex = 0;
    public bool CanShoot { get; set; } = true;
    public bool CanMelee { get; set; } = true;
    [field: SerializeField] public bool LookInCameraDirection { get; set; }
    
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
    
    
    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapons>();
    }
    
    #region AnimationEvents
    public void Sheath(int index)
    {
        Debug.Log("ASD");
        GameObject weaponMesh = Weapons[index].WeaponMesh;
        weaponMesh.SetActive(false);
    }

    public void UnSheath(int index)
    {
        Debug.Log("ASDA");
        GameObject weaponMesh = Weapons[index].WeaponMesh;
        weaponMesh.SetActive(true);
    }

    public void DisableTrigger(int index)
    {
        foreach (Collider collider in Weapons[index].WeaponColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableTrigger(int index)
    {
        foreach (Collider collider in Weapons[index].WeaponColliders)
        {
            collider.enabled = true;
        }
    }
    #endregion
}
