using UnityEngine;

// interfaces can implement other interfaces
// but they can't inherit from classes
public interface IDamageable
{
    // all properties/methods in interfaces are default public

    // fields are NOT supported in intercaces
    // float SomeFloat;

    // but properties are allowed
    float CurrentPercentage { get; }
    bool IsAlive { get; }

    // we can also add methods (functions)
    // typically methods in interfaces don't contain bodies
    // newer versions of C# allow us to define method bodies in interfaces
    // I haven't found a great use case for this
    void Damage(DamageInfo damageInfo);
}