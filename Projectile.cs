using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float damage;
    private Rigidbody rigidBody;
    private float modulus;
    private float altitude;
    private bool altitudeSet;
    private float angle;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        altitudeSet = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        angle = Vector3.Angle(transform.position, -transform.forward);
    }

    // Update is called once per frame
    void Update()
    {
        if (!altitudeSet) return;
        if (altitude <= 0)
        {
            Destroy(gameObject);
            return;
        }
        transform.rotation = Quaternion.LookRotation(-transform.position, transform.up);
        transform.Rotate(speed * Mathf.Rad2Deg * Mathf.Cos(angle * Mathf.Deg2Rad) * Time.deltaTime / (modulus + altitude), 0, 0);
        transform.position = -transform.forward * (modulus + altitude);
        transform.Rotate(angle, 0, 0, Space.Self);
        altitude -= speed * Mathf.Sin(angle * Mathf.Deg2Rad) * Time.deltaTime;
    }

    public void SetAltitude(float a)
    {
        modulus = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y + transform.position.z * transform.position.z) - a;
        altitude = a;
        altitudeSet = true;
    }
}
