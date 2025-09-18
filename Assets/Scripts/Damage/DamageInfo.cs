using UnityEngine;

public class DamageInfo
{
    public DamageInfo(float amount, DamageType damageType, bool isCrit, GameObject victim, GameObject source, GameObject instigator, float knockBackForce)
    {
        Amount = amount;
        DamageType = damageType;
        IsCrit = isCrit;
        Victim = victim;
        Source = source;
        Instigator = instigator;
        KnockBackForce = knockBackForce;
    }

    public float Amount { get; set; } = 0;
    public DamageType DamageType { get; set; } = DamageType.None;
    public bool IsCrit { get; set; } = false;
    public GameObject Victim { get; set; } = null;
    public GameObject Source { get; set; } = null;
    public GameObject Instigator { get; set; } = null;
    public float KnockBackForce { get; set; } = 0;
}
