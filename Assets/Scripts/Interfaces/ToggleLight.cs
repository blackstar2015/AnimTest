using UnityEngine;

[RequireComponent(typeof(Light))]
public class ToggleLight : MonoBehaviour, IDamageable, IInteractable
{
    // ? : shorthand reads if [condition] then first option, else second option
    public float CurrentPercentage => _light.enabled ? 1f : 0f;
    public bool IsAlive => _light.enabled;

    public float InteractRange { get; }
    public bool IsInteractable => true;
    public string InteractMessage => $"Toggle Light [{_messageState}]";
    private string _messageState => _light.enabled ? "Off" : "On";

    private Light _light;
    private float _toggleCooldown = 1f;
    private float _lastToggleTime;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    public void Damage(DamageInfo damageInfo)
    {
        // simple cooldown like our Weapon class
        if (Time.time < _lastToggleTime + _toggleCooldown) return;
        _lastToggleTime = Time.time;

        // flip flop light
        _light.enabled = !_light.enabled;
    }

    public void Interact(GameObject interactor)
    {
        _light.enabled = !_light.enabled;
    }
}