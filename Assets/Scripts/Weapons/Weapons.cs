using Sirenix.OdinInspector;
using UnityEngine;

public abstract class Weapons : MonoBehaviour
{
    [field: SerializeField, Required, InlineEditor] public WeaponData Data { get; private set; }

    private float _lastAttackTime = -100000f;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
    }

// attempt attack while respecting cooldown or other limiting factors
    public bool TryAttack(Vector3 aimPosition, GameObject instigator, int team)
    {
        // common simple cooldown pattern
        float cooldown = 1f / Data.AttackRate;
        float nextAttackTime = _lastAttackTime + cooldown;
        if(Time.time >= nextAttackTime)
        {
            _lastAttackTime = Time.time;
            Attack(aimPosition, instigator, team);
            return true;
        }

        return false;
    }

    protected virtual void Attack(Vector3 aimPosition, GameObject instigator, int team)
    {
        // play audio
        //if (!Data.SFX.IsNull) RuntimeManager.PlayOneShot(Data.SFX, transform.position);

        // play animation
        if (!string.IsNullOrEmpty(Data.AttackAnimName)) _animator.SetTrigger(Data.AttackAnimName);
    }
}
