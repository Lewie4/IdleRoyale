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
        if(target != null && target.currentStats.health <= 0 )
        {
            target = null;
            trackingTarget = false;
        }

        if (!inQueue && target == null)
        {
            GameManager.Instance.RequestTarget(this);
            inQueue = true;
        }

        if(!trackingTarget || Vector3.Distance(transform.position, huntPoint) < 25f)
        {
            Vector3 newPoint = RandomNavSphere(GameManager.Instance.Circle.position, 100 * GameManager.Instance.Circle.localScale.x, -1);
            huntPoint = newPoint;

            trackingTarget = true;
        }

        if(attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime / currentStats.attackSpeed;
        }

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

        if (target != null)
        {
            trackingTarget = true;
        }
    }

    public bool TakeDamage(Player attacker)
    {
        float damage = Random.Range(attacker.currentStats.damage.x, attacker.currentStats.damage.y);

        ChangeHealth(damage);

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

    public void Heal(float amount)
    {
        ChangeHealth(-amount);
    }

    private void ChangeHealth(float damage)
    {
        currentStats.health -= damage;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, startingStats.health);

        rend.material.color = Color.Lerp(Color.red, Color.white, currentStats.health / startingStats.health);
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }

    private void OnTriggerStay(Collider other)
    {
        //RedZone
    }
}
