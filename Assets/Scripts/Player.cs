using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class Stats
    {
        public float health;
        public Vector2 damage;
        public float movementSpeed;
        public float attackSpeed;
        public float range;

        public Stats(Stats stats)
        {
            health = stats.health;
            damage = stats.damage;
            movementSpeed = stats.movementSpeed;
            attackSpeed = stats.attackSpeed;
            range = stats.range;
        }
    }

    public string playerName;

    [SerializeField] Stats startingStats;
    public Stats currentStats;

    public int kills;

    Player target;
    bool trackingTarget;
    Vector3 huntPoint;

    FieldOfView FOV;

    bool inQueue;

    float attackCooldown;

    Renderer rend;

    NavMeshAgent navMeshAgent;

    private void Awake()
    {
        FOV = GetComponent<FieldOfView>();
        rend = GetComponent<Renderer>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        currentStats = new Stats(startingStats);

        navMeshAgent.speed = currentStats.movementSpeed;
    }

    void Update()
    {
        if(target != null && target.currentStats.health < 0 )
        {
            target = null;
            trackingTarget = false;
        }

        if (!inQueue && target == null)
        {
            GameManager.Instance.RequestTarget(this);
            inQueue = true;
        }

        if(!trackingTarget || Vector3.Distance(transform.position, huntPoint) < 10f)
        {
            Vector2 randomArea = Random.insideUnitCircle * 100;
            Vector3 newPoint = new Vector3(Mathf.Clamp(transform.position.x + randomArea.x, -GameManager.Instance.SpawnArea.x, GameManager.Instance.SpawnArea.x), transform.position.y, Mathf.Clamp(transform.position.z + randomArea.y, -GameManager.Instance.SpawnArea.y, GameManager.Instance.SpawnArea.y));
            huntPoint = newPoint;
        }

        if(attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime / currentStats.attackSpeed;
        }

        //transform.LookAt(target != null ? target.transform.position : huntPoint);
        navMeshAgent.destination = target != null ? target.transform.position : huntPoint;
        Debug.DrawLine(transform.position, navMeshAgent.destination);

        if (target != null)
        { 
            if(attackCooldown <= 0 && Vector3.Distance(transform.position, target.transform.position) <= currentStats.range)
            {
                if(target.TakeDamage(this))
                {
                    kills++;
                    GameManager.Instance.UpdateLeaderboard(LeaderboardType.Kills);
                }

                attackCooldown = 1;
            }
        }
    }

    void FixedUpdate()
    {
        var velocity = transform.forward * Mathf.Lerp(currentStats.movementSpeed * 0.5f, currentStats.movementSpeed, currentStats.health/startingStats.health);
        transform.position += velocity * Time.fixedDeltaTime;
    }

    public void GetTarget()
    {
        target = FOV.GetTarget();
        inQueue = false;
        trackingTarget = true;        
    }

    public bool TakeDamage(Player attacker)
    {
        float damage = Random.Range(attacker.currentStats.damage.x, attacker.currentStats.damage.y);
        currentStats.health -= damage;

        rend.material.color = Color.Lerp(Color.red, Color.white, currentStats.health / startingStats.health);

        if (currentStats.health <= 0)
        {
            Die();

            return true;
        }
        else
        {
            if(target == null || Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, attacker.transform.position))
            {
                target = attacker;
            }
        }

        return false;
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}
