using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TDManager : MonoBehaviour
{
    public GameObject towerBuyPanel;
    public GameObject moneyText;
    public GameObject priceText;
    public GameObject map;
    public GameObject playerBase;
    public GameObject[] enemyBases;
    public float mapRadius;
    public float altitude;
    public float rollSpeed;
    public int initialMoney;
    public float enemySpawnInterval;
    public GameObject towerPlaceholderPrefab;
    public GameObject towerPrefab;
    public GameObject enemyPrefab;
    private Text textMoney;
    private Text textPrice;
    private bool rollingUp;
    private bool rollingDown;
    private bool rollingLeft;
    private bool rollingRight;
    private int money;
    private List<Tower> towers;
    private List<Enemy> enemies;
    private GameObject currentPurchase;
    private int selectedTower;
    private float enemySpawnTime;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = map.transform.position - transform.forward * (mapRadius + altitude);
        towerBuyPanel.SetActive(false);
        textMoney = moneyText.GetComponent<Text>();
        textPrice = priceText.GetComponent<Text>();
        textMoney.text = "$" + initialMoney;
        playerBase.transform.position = map.transform.position - playerBase.transform.forward * mapRadius;
        for(int i = 0; i < enemyBases.Length; i++) enemyBases[i].transform.position = map.transform.position - enemyBases[i].transform.forward * mapRadius;
        money = initialMoney;
        towers = new List<Tower>();
        enemies = new List<Enemy>();
        rollingUp = false;
        rollingDown = false;
        rollingLeft = false;
        rollingRight = false;
        currentPurchase = null;
        selectedTower = -1;
        enemySpawnTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!rollingUp && !rollingDown && Input.GetKeyDown(KeyCode.UpArrow)) rollingUp = true;
        else if (rollingUp && Input.GetKeyUp(KeyCode.UpArrow)) rollingUp = false;
        if (!rollingUp && !rollingDown && Input.GetKeyDown(KeyCode.DownArrow)) rollingDown = true;
        else if (rollingDown && Input.GetKeyUp(KeyCode.DownArrow)) rollingDown = false;
        if (!rollingLeft && !rollingRight && Input.GetKeyDown(KeyCode.LeftArrow)) rollingLeft = true;
        else if (rollingLeft && Input.GetKeyUp(KeyCode.LeftArrow)) rollingLeft = false;
        if (!rollingLeft && !rollingRight && Input.GetKeyDown(KeyCode.RightArrow)) rollingRight = true;
        else if (rollingRight && Input.GetKeyUp(KeyCode.RightArrow)) rollingRight = false;
        if (rollingUp || rollingDown || rollingLeft || rollingRight)
        {
            float rotation = rollSpeed * Time.deltaTime * 180f / ((mapRadius + altitude) * Mathf.PI * ((rollingUp || rollingDown) && (rollingLeft || rollingRight) ? Mathf.Sqrt(2) : 1f));
            transform.Rotate(rollingUp ? rotation : rollingDown ? -rotation : 0, rollingLeft ? rotation : rollingRight ? -rotation : 0, 0, Space.Self);
            transform.position = map.transform.position - transform.forward * (mapRadius + altitude);
        }
        enemySpawnTime += Time.deltaTime;
        if (enemySpawnTime >= enemySpawnInterval)
        {
            enemySpawnTime = 0;
            GameObject b = enemyBases[Random.Range(0, enemyBases.Length)];
            Enemy newEnemy = Instantiate(enemyPrefab, b.transform.position, b.transform.rotation).GetComponent<Enemy>();
            newEnemy.SetTarget(playerBase.transform.position);
            enemies.Add(newEnemy);
        }
        List<Enemy> enemiesToRemove = new List<Enemy>();
        foreach (Enemy e in enemies)
        {
            if (e.GetDefeated() || e.GetReachedTarget()) enemiesToRemove.Add(e);
        }
        foreach (Enemy e in enemiesToRemove)
        {
            if (e.GetDefeated())
            {
                money += e.reward;
                textMoney.text = "$" + money;
            }
            Destroy(e.gameObject);
            enemies.Remove(e);
        }
        foreach (Tower t in towers)
        {
            Vector3 closestEnemy = new Vector3();
            float minDistance = -1;
            foreach (Enemy e in enemies)
            {
                float angle = Mathf.Abs(Vector3.Angle(t.transform.position, e.transform.position)) * Mathf.Deg2Rad;
                if (angle <= t.range / mapRadius && (minDistance < 0 || angle < minDistance))
                {
                    closestEnemy = e.transform.position.normalized;
                    minDistance = angle;
                }
            }
            if (minDistance < 0) t.SetNoTarget();
            else t.SetTarget(closestEnemy.normalized);
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            GameObject hitObject = hit.transform.gameObject;
            if (currentPurchase == null && Input.GetMouseButtonDown(0))
            {
                if (hitObject.GetComponent<Tower>() != null)
                {
                    for (int i = 0; i < towers.Count; i++)
                    {
                        if (hitObject == towers[i].gameObject)
                        {
                            selectedTower = i;
                            towers[i].SetRangeColour(Color.white);
                        }
                        else if (i == selectedTower)
                        {
                            towers[i].SetRangeColour(Color.clear);
                            selectedTower = -1;
                        }
                    }
                }
                else if(selectedTower >= 0)
                {
                    towers[selectedTower].SetRangeColour(Color.clear);
                    selectedTower = -1;
                }
            }
            else if(currentPurchase != null)
            {
                currentPurchase.transform.position = hit.point;
                currentPurchase.transform.rotation = Quaternion.LookRotation(map.transform.position - currentPurchase.transform.position, currentPurchase.transform.up);
                Tower t = currentPurchase.GetComponent<Tower>();
                bool valid = money >= t.price && hitObject == map;
                t.SetRangeColour(valid ? Color.white : Color.red);
                if (valid && Input.GetMouseButtonDown(0))
                {
                    towerBuyPanel.SetActive(false);
                    money -= t.price;
                    textMoney.text = "$" + money;
                    t.DestroyTower();
                    Tower newTower = Instantiate(towerPrefab, (currentPurchase.transform.position - map.transform.position).normalized * mapRadius, currentPurchase.transform.rotation).GetComponent<Tower>();
                    newTower.SetBought();
                    towers.Add(newTower);
                    currentPurchase = null;
                }
            }
        }
        else if (currentPurchase == null && selectedTower >= 0 && Input.GetMouseButtonDown(0))
        {
            towers[selectedTower].SetRangeColour(Color.clear);
            selectedTower = -1;
        }
        else if (currentPurchase)
        {
            currentPurchase.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (mapRadius + altitude) / 2));
            currentPurchase.transform.rotation = Quaternion.identity;
            currentPurchase.GetComponent<Tower>().SetRangeColour(Color.clear);
        }
    }

    public void BuyTower()
    {
        if (selectedTower >= 0)
        {
            towers[selectedTower].SetRangeColour(Color.clear);
            selectedTower = -1;
        }
        if (currentPurchase != null) CancelBuy();
        towerBuyPanel.SetActive(true);
        currentPurchase = Instantiate(towerPlaceholderPrefab, map.transform.position - transform.forward * mapRadius, transform.rotation);
        textPrice.text = "$" + currentPurchase.GetComponent<Tower>().price;
    }

    public void CancelBuy()
    {
        if (currentPurchase == null) return;
        towerBuyPanel.SetActive(false);
        currentPurchase.GetComponent<Tower>().DestroyTower();
        currentPurchase = null;
    }
}
