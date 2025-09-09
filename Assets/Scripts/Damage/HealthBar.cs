using System;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillBar;
    private Health _health;

    private void Start()
    {
        _health = GetComponentInParent<Health>();
        _health.OnDamage.AddListener(Damage);
        _health.OnDeath.AddListener(Death);

        _fillBar.fillAmount = 1;
    }

    private void Damage(DamageInfo damageInfo)
    {
        _fillBar.fillAmount = _health.CurrentPercentage;
    }

    private void Death(DamageInfo damageInfo)
    {
        _health.gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
