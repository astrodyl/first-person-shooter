using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField]
    private float defaultSpeed = 5f;

    [SerializeField]
    private float mouseSensitivity = 0.5f;

    [SerializeField]
    private float jumpForce = 10f;

    private PlayerMotor motor;

    // Weapon
    [SerializeField] Weapon[] weapons;
    int weaponIndex;
    int previousWeaponIndex = -1;

    // Ground
    public Transform groundCheck;
    public float groundDistance = 4f;
    public LayerMask groundMask;

    PhotonView PV;

    const float maxHealth = 100f;
    float currentHealth = maxHealth;

    PlayerManager playerManager;

    // Audio
    AudioSource walkingSound;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        motor = GetComponent<PlayerMotor>();
        PV = GetComponent<PhotonView>();

        walkingSound = GetComponent<AudioSource>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        if (PV.IsMine)
        {
            // Equip first item
            EquipWeapon(0);
        }
        else
        {
            // Delete cameras not belonging to owner
            foreach(Transform child in transform)
            {
                if (child.name == "CameraHolder")
                {
                    foreach (Transform grandchild in child)
                    {
                        if (grandchild.name == "GunCamera" || grandchild.name == "MainCamera")
                        {
                            Destroy(grandchild.gameObject);
                        }
                    }
                }
            }
            Destroy(motor);
        }
    }

    void Update()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }

        if (isWalking)
        {
            //walkingSound.Play();
        }


        float _speed = defaultSpeed;
        bool _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        /* CALCULATE VELOCITY AND APPLY */
        #region VELOCITY
        float _x = Input.GetAxisRaw("Horizontal");
        float _z = Input.GetAxisRaw("Vertical");

        CalculateMovementSpeed(ref _speed);
        Vector3 _velocity = (transform.right * _x + transform.forward * _z).normalized * _speed;

        motor.Move(_velocity);
        #endregion VELOCITY

        /* CALCULATE PLAYER ROTATION AND APPLY */
        #region ROTATION
        float _yRot = Input.GetAxisRaw("Mouse X");
        Vector3 _rotation = new Vector3(0f, _yRot, 0f) * mouseSensitivity;

        motor.Rotate(_rotation);
        

        /* CALCULATE CAMERA ROTATION AND APPLY */
        float _xRot = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRot * mouseSensitivity;

        motor.RotateCamera(_cameraRotationX);
        #endregion ROTATION

        /* PLAYER MECHANICS */
        #region JUMP    
        Vector3 _jumpForce = Vector3.zero;
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _jumpForce = Vector3.up * jumpForce;
            motor.Jump(_jumpForce);
        }
        #endregion

        #region WEAPON
        for (int i=0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown((i+1).ToString()))
            {
                EquipWeapon(i);
                break;
            }
        }
        #endregion

    }

    void EquipWeapon(int _index)
    {
        if (_index == previousWeaponIndex)
        {
            return;
        }

        weaponIndex = _index;
        weapons[weaponIndex].gameObject.SetActive(true);

        if (previousWeaponIndex != -1)
        {
            weapons[previousWeaponIndex].gameObject.SetActive(false);
        }
        previousWeaponIndex = weaponIndex;

        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    private void CalculateMovementSpeed(ref float _speed)
    {
        bool _isBackwards = Input.GetKey(KeyCode.S);

        if (isSprinting)
        {
            if (_isBackwards)
            {
                _speed = defaultSpeed / 1.5f;
            }
            else
            {
                _speed = defaultSpeed * 2f;
            }
        }
        else
        {
            if (_isBackwards)
            {
                _speed = defaultSpeed / 2f;
            }
            else
            {
                _speed = defaultSpeed;
            }
        }
    }

    public bool isSprinting
    {
        get
        {
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
            {
                return true;
            }
            return false;
        }
    }

    public bool isWalking
    {
        get
        {
            if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
            {
                return true;
            }
            return false;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipWeapon((int)changedProps["weaponIndex"]);
        }
    }

    public void TakeDamage(float _damage)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, _damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float _damage)
    {
        // Only take damage if other player
        if (!PV.IsMine) {
            return;
        }
        currentHealth -= _damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }
}
