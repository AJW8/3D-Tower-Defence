using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float initialHealth;
    public float speed;
    public int reward;
    private Rigidbody rigidBody;
    private float health;
    private Vector3 target;
    private float modulus;
    private bool reachedTarget;

    void Awake()
    {
        health = initialHealth;
        target = new Vector3();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        modulus = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y + transform.position.z * transform.position.z);
        reachedTarget = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (reachedTarget) return;
        reachedTarget = Mathf.Abs(Vector3.Angle(transform.position, target)) * Mathf.Deg2Rad <= speed * Time.deltaTime / modulus;
        transform.rotation = Quaternion.LookRotation(transform.forward, target.normalized - transform.position.normalized);
        transform.Rotate(speed * Mathf.Rad2Deg * Time.deltaTime / modulus, 0, 0, Space.Self);
        transform.position = -transform.forward * modulus;
    }

    public bool GetDefeated()
    {
        return health == 0;
    }

    public bool GetReachedTarget()
    {
        return reachedTarget;
    }

    public void SetTarget(Vector3 t)
    {
        target = t;
    }

    void OnCollisionEnter(Collision collision)
    {
        Projectile p = collision.gameObject.GetComponent<Projectile>();
        if (p == null) return;
        health -= p.damage;
        Destroy(collision.gameObject);
    }
}
