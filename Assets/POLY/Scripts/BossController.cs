﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    public static BossController instance;
    [SerializeField] BossAction[] actions;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] int currentBossHealth;
    [SerializeField] GameObject deathEffect;
    [SerializeField] GameObject bossCanvas;
    [SerializeField] Image bossHealthSlider;
    [SerializeField] float bossAgroDistance;
    public GameObject hitEffect;
    int maxBossHealth;
    
    Vector2 moveDirection;
    float actionCounter;
    float shootCounter;
    int currentAction;

    private void Awake() {
        instance = this;
        maxBossHealth = currentBossHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        actionCounter = actions[currentAction].actionLength;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(PlayerController.instance.transform.position, transform.position) > bossAgroDistance) return;

        if(actionCounter > 0)
        {
            actionCounter -= Time.deltaTime;
            
            //Movement
            moveDirection = Vector2.zero;

            if(actions[currentAction].shouldChasePlayer)
            {
                LookAt(PlayerController.instance.transform);
                moveDirection = PlayerController.instance.transform.position - transform.position;
                moveDirection.Normalize();
            }

            if(actions[currentAction].moveToPoint)
            {
                moveDirection = actions[currentAction].pointToMoveTo.position - transform.position;
            }


            //Shooting
            if(actions[currentAction].shouldShoot)
            {
                shootCounter -= Time.deltaTime;
                if(shootCounter<=0)
                {
                    shootCounter = actions[currentAction].timeBetweenShoots;

                    LookAt(PlayerController.instance.transform);

                    foreach(Transform t in actions[currentAction].shootPoints)
                    {
                        Instantiate(actions[currentAction].itemToShoot, t.position, t.rotation);
                    }
                }
            }

            rigidbody.velocity = moveDirection * actions[currentAction].moveSpeed;
        }
        else
        {
            currentAction++;
            if(currentAction >= actions.Length)
            {
                currentAction = 0;
            }

            actionCounter = actions[currentAction].actionLength;
        }
        
        bossCanvas.transform.position = new Vector2(transform.position.x, transform.position.y);
    }

    public void TakeDamage(int damageAmount)
    {
        currentBossHealth -= damageAmount;
        
        if(currentBossHealth <= 0)
        {
            gameObject.SetActive(false);
            bossCanvas.SetActive(false);

            Instantiate(deathEffect, transform.position, transform.rotation);

            LevelManager.instance.StartCoroutine("LevelEnd");
        }

        bossHealthSlider.fillAmount = (float) currentBossHealth / maxBossHealth;
    }

    private void LookAt(Transform target)
    {
        Vector2 rotateDirection = new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y);
        var angle = Mathf.Atan2(rotateDirection.y, rotateDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

[System.Serializable]
public class BossAction
{
    [Header("Action")]
    public float actionLength;
    public bool shouldMove;
    public bool shouldChasePlayer;
    public float moveSpeed;
    public bool moveToPoint;
    public Transform pointToMoveTo;
    public bool shouldShoot;
    public GameObject itemToShoot;
    public float timeBetweenShoots;
    public Transform[] shootPoints;
}
