using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class WeaponController : MonoBehaviour
{
    
    private bool isReloading = false;
    public float aimFov = 40f;

    [Header("Camera Options")]
    public Camera mainCamera;
    public Camera gunCamera;
    [Tooltip("How fast the camera field of view changes when aiming.")]
    public float fovSpeed = 15.0f;
    [Tooltip("Default value for camera field of view.")]
    public float defaultFov = 60.0f;

    [Header("Fire Rate")]

    public float timeBetweenShots;

    [Header("Ammo")]
    public float bulletsPerTap;
    int bulletsLeft, bulletsShot;

    // Recoil
    int index;

    [Header("Muzzleflash Settings")]
    public bool randomMuzzleflash = false;
    //min should always bee 1
    private int minRandomValue = 1;

    [Range(2, 25)]
    public int maxRandomValue = 5;

    private int randomMuzzleflashValue;

    public bool enableMuzzleflash = true;
    public bool enableSparks = true;
    public int minSparkEmission = 1;
    public int maxSparkEmission = 7;

    [Header("Muzzleflash Light Settings")]
    public float lightDuration = 0.02f;

    [Header("Misc")]
    public bool allowInvoke;
    public int ammoRemaining;
    private AudioSource shootAudioSource;

    [Header("References")]
    public Transform bulletPrefab;
    private Transform bulletSpawnPoint;
    public PlayerMotor motor;

    private Weapon weapon;
    private string equippedWeaponName = "None";
    private bool readyToShoot, shooting;
    private PhotonView PV;

    private Quaternion originalRotation;
    private Vector3 originalPosition;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (!PV.IsMine)
        {
            return;
        }
        shootAudioSource = GetComponent<AudioSource>();
        originalRotation = gunCamera.transform.localRotation;
        originalPosition = gunCamera.transform.localPosition;
        readyToShoot = true;
    }

    void Update()
    {

        if (!PV.IsMine) {
            return;
        }

        weapon = GetComponentInChildren<Weapon>();
        
        if (weapon == null) {
            return;
        }

        // Update newly equipped weapon
        if (weapon.weaponInfo.name != equippedWeaponName)
        {
            weapon = GetComponentInChildren<Weapon>();

            if (equippedWeaponName == "None")
            {
                bulletsLeft = weapon.weaponInfo.magazineSize;
            }
            if (bulletsLeft > weapon.weaponInfo.magazineSize) // hardcoded nonsense
            {
                bulletsLeft = weapon.weaponInfo.magazineSize;
            }
            equippedWeaponName = weapon.weaponInfo.name;
            weapon.muzzleflashLight.enabled = false;
        }

        if (weapon.weaponInfo.isFullAuto) // move to weapon info
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //If randomize muzzleflash is true, genereate random int values
        if (randomMuzzleflash == true)
        {
            randomMuzzleflashValue = Random.Range(minRandomValue, maxRandomValue);
        }

        // Aiming
        if (Input.GetButton("Fire2") && !isReloading)
        {
            SetCameraFov(aimFov, weapon.weaponInfo.aimFov);
            MoveWeapon(weapon.weaponInfo.aimPosition, weapon.weaponInfo.aimRotation);
        }
        else
        {
            SetCameraFov(defaultFov, defaultFov);
            MoveWeapon(weapon.weaponInfo.hipPosition, weapon.weaponInfo.hipRotation);
        }

        // Reloading
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < weapon.weaponInfo.magazineSize && !isReloading && ammoRemaining > 0)
        {
            Reload();
        }

        // Reload automatically if trying to shoot without ammo
        if (readyToShoot && shooting && !isReloading && bulletsLeft <= 0 && ammoRemaining > 0)
        {
            Reload();
        }

        // Shooting
        if (readyToShoot && shooting && !isReloading && bulletsLeft > 0)
        {
            bulletsShot = 0;
            Shoot();
        }

        if (isReloading)
        {
            weapon.transform.localPosition = weapon.transform.localPosition + new Vector3(0f, -0.03f, 0f);
        }

    }

    void Shoot()
    {
        readyToShoot = false;

        // Tell the bullet script how much damage it does
        bulletPrefab.GetComponent<BulletScript>().damage = weapon.weaponInfo.damage;

        bulletSpawnPoint = weapon.bulletSpawn;

        // Spawn bullet from bullet spawnpoint
        var bullet = (Transform)Instantiate(
            bulletPrefab,
            bulletSpawnPoint.transform.position,
            bulletSpawnPoint.transform.rotation);

        //var bullet = PhotonNetwork.Instantiate(
            //"BulletPrefab",
            //bulletSpawnPoint.transform.position,
            //bulletSpawnPoint.transform.rotation);

        // Bullet spread
        float _x = 0f;
        float _y = 0f;

        if (!isAiming)
        {
            if (!randomMuzzleflash && enableMuzzleflash == true)
            {
                weapon.muzzleParticles.Emit(1);
                StartCoroutine(MuzzleFlashLight());
            }
            else if (randomMuzzleflash == true)
            {
                //Only emit if random value is 1
                if (randomMuzzleflashValue == 1)
                {
                    if (enableSparks == true)
                    {
                        //Emit random amount of spark particles
                        weapon.sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                    }
                    if (enableMuzzleflash == true)
                    {
                        weapon.muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(MuzzleFlashLight());
                    }
                }
            }
            // Increase spread if not aiming
            _x = Random.Range(-1f, 1f) * weapon.weaponInfo.bulletSpread;
            _y = Random.Range(-1f, 1f) * weapon.weaponInfo.bulletSpread;
        }
        else
        {
            if (!randomMuzzleflash)
            {
                weapon.muzzleParticles.Emit(1);
                //If random muzzle is true
            }
            else if (randomMuzzleflash == true)
            {
                //Only emit if random value is 1
                if (randomMuzzleflashValue == 1)
                {
                    if (enableSparks == true)
                    {
                        //Emit random amount of spark particles
                        weapon.sparkParticles.Emit(Random.Range(minSparkEmission, maxSparkEmission));
                    }
                    if (enableMuzzleflash == true)
                    {
                        weapon.muzzleParticles.Emit(1);
                        //Light flash start
                        StartCoroutine(MuzzleFlashLight());
                    }
                }
            }
        }
        bullet.transform.Rotate(_x, _y, 0f);

        // Add force to the bullet
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * weapon.weaponInfo.bulletForce, ForceMode.Impulse);

        // Simulate weapon recoil
        AddRecoil();

        // Add audio
        if (shootAudioSource.clip != weapon.weaponInfo.shootingSound)
        {
            shootAudioSource.clip = weapon.weaponInfo.shootingSound;
        }
        
        shootAudioSource.Play();

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            Invoke("ResetShot", 1f / weapon.weaponInfo.fireRate);
            allowInvoke = false;
        }

        // If more than one bulletsPerTap - i.e., shotgun spread
        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;

        // Return weapon to orginal position
        ResetRecoil();
    }

    private void AddRecoil()
    {
        // Rotate weapon
        weapon.gameObject.transform.localRotation = weapon.gameObject.transform.localRotation * Quaternion.Euler(new Vector3(-7f, 0f, 0f));
        // Move weapon backwards
        gunCamera.transform.localPosition = Vector3.Lerp(gunCamera.transform.localPosition, gunCamera.transform.localPosition + new Vector3(0f, 0f, 0.025f), 1f);
        // Lerping is too slow. Immediately set new fov and lerp back.
        mainCamera.fieldOfView = mainCamera.fieldOfView + 1f;
    }

    private void ResetRecoil()
    {
        gunCamera.transform.localRotation = originalRotation;
        gunCamera.transform.localPosition = originalPosition;
    }

    private void Reload()
    {
        if (shootAudioSource.clip != weapon.weaponInfo.reloadSound)
        {
            shootAudioSource.clip = weapon.weaponInfo.reloadSound;
        }
        shootAudioSource.Play();

        isReloading = true;
        Invoke("ReloadFinished", weapon.weaponInfo.reloadTime);
    }

    private void ReloadFinished()
    {
        if (ammoRemaining >= weapon.weaponInfo.magazineSize)
        {
            ammoRemaining -= weapon.weaponInfo.magazineSize - bulletsLeft;
            bulletsLeft = weapon.weaponInfo.magazineSize;
        }
        else
        {
            bulletsLeft = ammoRemaining;
            ammoRemaining = 0;
        }
        isReloading = false;
    }

    private void MoveWeapon(Vector3 _newPos, Vector3 _newRot)
    {
        Vector3 _currentPos = weapon.transform.localPosition;
        float _aimSpeed = weapon.weaponInfo.aimSpeed;

        weapon.transform.localPosition = Vector3.MoveTowards(_currentPos, _newPos, _aimSpeed * Time.deltaTime);
        weapon.transform.localRotation = Quaternion.RotateTowards(weapon.transform.localRotation, Quaternion.Euler(_newRot), _aimSpeed * 15f * Time.deltaTime);
    }

    private void SetCameraFov(float _newMainFov, float _newGunFov)
    {
        // Main Camera
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView,
                _newMainFov, fovSpeed * Time.deltaTime);
        // Gun Camera
        gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView,
                _newGunFov, fovSpeed * Time.deltaTime);
    }

    private IEnumerator MuzzleFlashLight()
    {

        weapon.muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        weapon.muzzleflashLight.enabled = false;
    }

    public bool isAiming
    {
        get
        {
            if (Input.GetButton("Fire2") && !isReloading)
            {
                return true;
            }
            return false;
        }
        set
        {
            isAiming = value;
        }
    }
}
