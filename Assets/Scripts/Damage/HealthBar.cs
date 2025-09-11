using System;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _fillHealthBar;
    [SerializeField] private Image _fillStaminaBar;
    private Health _health;

    private void Start()
    {
        _health = GetComponentInParent<Health>();
        _health.OnDamage.AddListener(Damage);
        _health.OnDeath.AddListener(Death);
        _health.OnBlock.AddListener(Damage);
        _health.OnUpdateStamina.AddListener(UpdateStamina);

        _fillHealthBar.fillAmount = 1;
        _fillStaminaBar.fillAmount = 1;
    }

    private void UpdateStamina()
    {
        _fillStaminaBar.fillAmount = _health.CurrentStaminaPercentage;
    }

    private void Damage(DamageInfo damageInfo)
    {
        _fillHealthBar.fillAmount = _health.CurrentHealthPercentage;
        _fillStaminaBar.fillAmount = _health.CurrentStaminaPercentage;
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
