using UnityEngine;
using Photon.Pun;

public class HeadBob : MonoBehaviour
{
    public float walkingBobbingSpeed = 15f;
    public float sprintingBobbingSpeed = 20f;
    public float defaultBobbingAmount = 0.1f;
    public PlayerController controller;
    public WeaponController weaponController;

    float bobbingSpeed;
    float defaultPosY = 0;
    float timer = 0;

    PhotonView PV;

    void Start()
    {
        PV = GetComponentInParent<PhotonView>();

        if (!PV.IsMine) {
            return;
        }

        defaultPosY = transform.localPosition.y;
    }

    void Update()
    {
        if (!PV.IsMine) {
            return;
        }

        if (controller.isWalking)
        {
            bobbingSpeed = walkingBobbingSpeed;
        }
        else if (controller.isSprinting)
        {
            bobbingSpeed = sprintingBobbingSpeed;
        }
        float bobbingAmount = defaultBobbingAmount;

        // Reduce bobbing effect if aiming down sights
        if (weaponController.isAiming)
        {
            bobbingSpeed *= 0.5f;
            bobbingAmount *= 0.2f;
        }

        if (controller.isSprinting || controller.isWalking)
        {
            //Player is moving
            timer += Time.deltaTime * bobbingSpeed;
            transform.localPosition = new Vector3(transform.localPosition.x, defaultPosY + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
        }
        else
        {
            //Idle
            timer = 0;
            transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(transform.localPosition.y, defaultPosY, Time.deltaTime * walkingBobbingSpeed), transform.localPosition.z);
        }
    }
}