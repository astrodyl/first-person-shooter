using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon")]
public class WeaponInfo : ScriptableObject
{
    // Defines a weapon

    [Header("UI Weapon Name")]
    new public string name = "New Weapon";

    [Header("Recoil")]
    public Vector2[] recoilPattern = new Vector2[10];

    [Header("Weapon Settings")]
    [Tooltip("Single tap or full auto")]
    public bool isFullAuto;
    [Tooltip("How fast the weapon fires, higher value means faster rate of fire.")]
    public float fireRate;
    [Tooltip("How much ammo the weapon should have.")]
    public int magazineSize;
    [Tooltip("How much damage each bullet does.")]
    public float damage;
    [Tooltip("How fast the weapon aims down sights")]
    public float aimSpeed;
    [Tooltip("How fast the weapon aims down sights")]
    public float reloadTime;

    [Header("Bullet Settings")]
    [Tooltip("How much force is applied to the bullet when shooting.")]
    public float bulletForce;
    [Tooltip("How much directional variation is applied to the when shooting.")]
    public float bulletSpread;

    [Header("Weapon Transforms")]
    [Tooltip("Weapon Position that makes gun point to center while aiming.")]
    public Vector3 aimPosition;
    public Vector3 aimRotation;

    [Tooltip("Weapon Position that makes gun point to center during hipfire.")]
    public Vector3 hipPosition;
    public Vector3 hipRotation;

    [Header("Gun Camera Transforms")]
    public float aimFov;

    [Header("Weapon Audio Clips")]
    public AudioClip shootingSound;
    public AudioClip reloadSound;

}
