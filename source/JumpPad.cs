using UnityEngine;
using System.Collections;

public class JumpPad : MonoBehaviour
{

    public float jumpForce = 1000f;
    public AudioSource sound;

    void Awake()
    {
        sound = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If bullet collides with "AimTrainerTarget" tag
        if (collision.transform.tag == "Player")
        {
            // Play jumppad audio
            sound.Play();
            // Apply upward force
            collision.gameObject.GetComponent<Rigidbody>().AddForce(collision.transform.up * jumpForce, ForceMode.Impulse);
        }
    }
}
