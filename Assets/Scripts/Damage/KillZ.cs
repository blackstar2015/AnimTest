using System;
using Unity.VisualScripting;
using UnityEngine;

public class KillZ : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health health))
        {
            DamageInfo damageInfo = new DamageInfo(100,DamageType.Fire,true,other.gameObject,gameObject,gameObject,0);
            health.OnDeath.Invoke(damageInfo);
        }
    }
}
