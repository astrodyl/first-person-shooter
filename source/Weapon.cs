using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponInfo weaponInfo;
    public Transform bulletSpawn;

    [Header("Muzzle Flash References")]
    public ParticleSystem muzzleParticles;
    public ParticleSystem sparkParticles;
    public Light muzzleflashLight;
}
