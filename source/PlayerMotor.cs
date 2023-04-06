using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    public GameObject cam;

    private Vector3 velocity = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;

    private Rigidbody rb;

    [SerializeField]
    private float cameraRotationLowerLimit = 30f;

    [SerializeField]
    private float cameraRotationUpperLimit = 75f;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>(); 
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCamera(float _cameraRotationX)
    {
        cameraRotationX = _cameraRotationX;
    }

    // Run every physics iteration
    void FixedUpdate()
    {
        if (!PV.IsMine)
        {
            return;
        }

        PerformMovement();
        PerformRotation();
    }

    void PerformMovement()
    {
        if (velocity != Vector3.zero)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }

    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));

        if (cam != null)
        {
            
            // Set rotation and clamp it
            currentCameraRotationX -= cameraRotationX;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationUpperLimit, cameraRotationLowerLimit);

            Vector3 angle = new Vector3(currentCameraRotationX, 0f, 0f);
            angle.x = Mathf.LerpAngle(cam.transform.localEulerAngles.x, angle.x, 20f );

            cam.transform.localEulerAngles = angle;

            //cam.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    public void Jump(Vector3 _jumpForce)
    {
        if (_jumpForce != Vector3.zero)
        {
            // ForceMode applies force at once instead over time
            rb.AddForce(_jumpForce, ForceMode.Impulse);
        }
    }

    public void ApplyRecoil(float xRecoil, float yRecoil)
    {
        if (cam != null)
        {
            // Vertical recoil
            currentCameraRotationX -= xRecoil;
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationUpperLimit, cameraRotationLowerLimit);
        }
    }

}
