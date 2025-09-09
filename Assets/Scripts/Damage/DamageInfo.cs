using UnityEngine;

public class DamageInfo
{
    public DamageInfo(float amount, DamageType damageType, bool isCrit, GameObject victim, GameObject source, GameObject instigator)
    {
        Amount = amount;
        DamageType = damageType;
        IsCrit = isCrit;
        Victim = victim;
        Source = source;
        Instigator = instigator;
    }

    public float Amount { get; set; }
    public DamageType DamageType { get; set; }
    public bool IsCrit {  get; set; }
    public GameObject Victim { get; set; }          
    public GameObject Source { get; set; }          
    public GameObject Instigator { get; set; }     
}
