using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public GameObject turret;
    public int rangePointCount;
    public int price;
    public float range;
    public float fireInterval;
    public float turretAngleConstant;
    public GameObject rangePointPrefab;
    public GameObject projectilePrefab;
    private GameObject[] rangePoints;
    private bool bought;
    private float modulus;
    private float turretHeight;
    private float fireTime;
    private bool target;

    void Awake()
    {
        rangePoints = new GameObject[rangePointCount];
        for (int i = 0; i < rangePointCount; i++) rangePoints[i] = Instantiate(rangePointPrefab, transform.position, transform.rotation);
        bought = false;
        fireTime = 0;
        target = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            fireTime += Time.deltaTime;
            if (fireTime >= fireInterval)
            {
                fireTime = 0;
                Projectile p = Instantiate(projectilePrefab, turret.transform.position, turret.transform.rotation).GetComponent<Projectile>();
                p.SetAltitude(turretHeight);
            }
        }
        else if(!bought)
        {
            Quaternion rotation = transform.rotation;
            float modulus = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y + transform.position.z * transform.position.z);
            for (int i = 0; i < rangePointCount; i++)
            {
                transform.Rotate(0, 0, i * 360f / rangePointCount, Space.Self);
                transform.Rotate(range * Mathf.Rad2Deg / modulus, 0, 0, Space.Self);
                rangePoints[i].transform.position = -transform.forward * modulus;
                rangePoints[i].transform.rotation = transform.rotation;
                transform.rotation = rotation;
            }
        }
    }

    public void SetRangeColour(Color c)
    {
        for (int i = 0; i < rangePointCount; i++) rangePoints[i].GetComponent<SpriteRenderer>().color = c;
    }

    public void SetBought()
    {
        bought = true;
        modulus = Mathf.Sqrt(transform.position.x * transform.position.x + transform.position.y * transform.position.y + transform.position.z * transform.position.z);
        turretHeight = Mathf.Sqrt(turret.transform.position.x * turret.transform.position.x + turret.transform.position.y * turret.transform.position.y + turret.transform.position.z * turret.transform.position.z) - modulus;
    }

    public void SetNoTarget()
    {
        target = false;
    }

    public void SetTarget(Vector3 t)
    {
        if (!bought) return;
        target = true;
        turret.transform.rotation = Quaternion.LookRotation(transform.forward, t.normalized - transform.position.normalized);
        turret.transform.Rotate((90f - Mathf.Abs(Vector3.Angle(transform.position, t)) / 2) * turretAngleConstant / modulus, 0, 0, Space.Self);
    }

    public void DestroyTower()
    {
        for (int i = 0; i < rangePointCount; i++) Destroy(rangePoints[i]);
        Destroy(gameObject);
    }
}
