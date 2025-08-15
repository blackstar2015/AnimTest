using LazyObjectPooler;
using UnityEngine;

public class WeaponsRanged : Weapons
{
    [field: SerializeField] public Transform Muzzle { get; private set; }

    public WeaponRangedProjectileData RangedData => (WeaponRangedProjectileData)Data;

    protected override void Attack(Vector3 aimPosition, GameObject instigator, int team)
    {
        base.Attack(aimPosition, instigator, team);

        Debug.DrawLine(transform.position, aimPosition, Color.red, 1f);
    
        // base aim values
        Vector3 spawnPos = Muzzle.position;
        Vector3 aimDir = (aimPosition - spawnPos).normalized;           // direction from A to B is B minus A, normalized
        Quaternion spawnRot = Quaternion.LookRotation(aimDir);          // LookRotation() turns a DIRECTION into A ROTATION
    
        for (int i = 0; i < RangedData.ShotCount; i++)
        {
            // randomly generate inaccuracy
            float inaccHorizontal = Random.Range(-RangedData.Inaccuracy, RangedData.Inaccuracy);
            float inaccVertical = Random.Range(-RangedData.Inaccuracy, RangedData.Inaccuracy);
    
            // create rotation from inaccuracy (more quaternion fun)
            Vector3 horizontalAngle = Muzzle.up * inaccHorizontal;
            Vector3 verticalAngle = Muzzle.right * inaccVertical;       // these are Euler angles! not quaternions!
            Quaternion inaccRotation = Quaternion.Euler(horizontalAngle + verticalAngle);   // Euler() turns (x,y,z) into a rotation (quaternion)
    
            // combine spawn rotation and inaccuracy rotation
            Quaternion finalRotation = spawnRot * inaccRotation;    // we multiply quaternions to combine their rotations
    
            // draw debug line for each bullet
            Vector3 bulletDir = finalRotation * Vector3.forward;    // multiplying a ROTATION by a DIRECTION gives the direction of that rotation
            Debug.DrawRay(spawnPos, bulletDir * RangedData.Range, Color.yellow, 1f);
    
            // spawn projectile and assign values
            //Projectile projectile = Instantiate(RangedData.BulletPrefab, spawnPos, finalRotation);     // instantiate
            Projectile projectile = PoolSystem.Instance.Get(RangedData.BulletPrefab, spawnPos, finalRotation) as Projectile;
            // Awake() is called
            projectile?.Launch(RangedData.Damage, RangedData.Range, RangedData.BulletSpeed, RangedData.DamageType, instigator, team);    // we call our code
            // Start() is called
        }
    }
}
