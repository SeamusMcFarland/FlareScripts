using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int monsterType;

    enum MonsterState { idle, attacking, struck, dead}; // specific to this creature // Moving and transform is NOT a state for MonsterState
    private MonsterState currentState;

    public GameObject monsterAO; // animation game object
    private AnimationScript monsterAS; // animation script

    private Rigidbody2D rb;

    private const float ACCELERATION = 6f;
    private const float MAX_SPEED = 1.5f;

    private GroundColliderScript gcS;

    private AudioControllerScript acS;

    private float attackAfterlagTimer;
    private const float ATTACK_AFTERLAG = 3f;
    private const float ATTACK_STARTUP = 0.2f;
    private const float ATTACK_LENGTH = 0.3f;
    public ProjectileManagerScript projectileMS;

    private float frameNormalizer;

    private const float STRUCK_COOLDOWN = 0.5f;
    private float struckTimer;
    public float health;

    private PlayerScript playerS;
    private const float SIGHT_DISTANCE = 6f;
    private const float MIN_DISTANCE = 2f;

    public float speedModifier;
    public float attackRateModifier;

    public MonsterHitboxScript hitboxS;

    void Start()
    {

        playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        gcS = GetComponentInChildren<GroundColliderScript>();
        acS = GameObject.FindGameObjectWithTag("Audio Controller").GetComponent<AudioControllerScript>();
        monsterAS = monsterAO.GetComponent<AnimationScript>();
        rb = GetComponent<Rigidbody2D>();

        currentState = MonsterState.idle;
    }

    void Update()
    {
        frameNormalizer = Time.deltaTime / 0.04f;

        struckTimer -= Time.deltaTime;
        attackAfterlagTimer -= Time.deltaTime;

        switch (currentState)
        {
            case MonsterState.attacking:
                CheckMovement();
                break;

            case MonsterState.dead:

                break;

            case MonsterState.idle:
                CheckIdleAnimation();
                CheckMovement();
                CheckAttack();
                break;

            case MonsterState.struck:

                break;

            default:
                Debug.LogError("WARNING! INVALID MonsterState IN UPDATE!");
                break;
        }

    }

    private void CheckMovement()
    {
            if (GetPlayerSeen() && Vector2.Distance(transform.position, playerS.transform.position) > MIN_DISTANCE)
                MoveTowardPlayer();
            else
                RandomMovement();
    }

    private void MoveTowardPlayer()
    {
        if (playerS.transform.position.y > transform.position.y && rb.velocity.y < MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(0, ACCELERATION * frameNormalizer));
        else if (playerS.transform.position.y < transform.position.y && rb.velocity.y > -MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(0, -ACCELERATION * frameNormalizer));
        if (playerS.transform.position.x > transform.position.x && rb.velocity.x < MAX_SPEED * speedModifier)
        {
            monsterAS.FlipTo(true);
            rb.AddForce(new Vector2(ACCELERATION * frameNormalizer, 0));
        }
        else if (playerS.transform.position.x < transform.position.x && rb.velocity.x > -MAX_SPEED * speedModifier)
        {
            monsterAS.FlipTo(false);
            rb.AddForce(new Vector2(-ACCELERATION * frameNormalizer, 0));
        }
    }

    private void RandomMovement()
    {
        if (Random.value < 0.5f && rb.velocity.y < MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(0, ACCELERATION * frameNormalizer));
        else if (rb.velocity.y > -MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(0, -ACCELERATION * frameNormalizer));
        if (Random.value < 0.5f && rb.velocity.x < MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(ACCELERATION * frameNormalizer, 0));
        else if (rb.velocity.x > -MAX_SPEED * speedModifier)
            rb.AddForce(new Vector2(-ACCELERATION * frameNormalizer, 0));
    }

    private void CheckIdleAnimation()
    {
        if (GetPlayerSeen())
            monsterAS.SetToAnimation(AnimationState.moving);
        else
            monsterAS.SetToAnimation(AnimationState.moving); // WAS idle
    }

    private bool GetPlayerSeen()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, playerS.transform.position - transform.position);
        if (Vector2.Distance(transform.position, playerS.transform.position) <= SIGHT_DISTANCE || (Vector2.Distance(transform.position, playerS.transform.position) <= SIGHT_DISTANCE * 3f && hit.transform.CompareTag("Player")))
            return true;
        else
            return false;
    }

    private void CheckAttack()
    {
        if (GetPlayerSeen() && attackAfterlagTimer <= 0)
        {
            attackAfterlagTimer = 100f; // locks until set to constant
            Attack();
        }
    }

    private void Attack()
    {
        monsterAS.SetToAnimation(AnimationState.attacking);
        currentState = MonsterState.attacking;
        StartCoroutine(UnlockFromAttack());
    }

    IEnumerator UnlockFromAttack()
    {
        yield return new WaitForSeconds(ATTACK_STARTUP);
        if (currentState == MonsterState.attacking)
        {
            if (monsterType == 1 || monsterType == 2)
                FireProjectile();
            else
            {
                if(monsterType == 3)
                    acS.PlaySound("Enemy Strike");
                else if(monsterType == 4)
                    acS.PlaySound("Enemy Zap");
                hitboxS.EnableHitbox();
            }
        }
        yield return new WaitForSeconds(ATTACK_LENGTH);
        if (!(monsterType == 1 || monsterType == 2))
            hitboxS.DisableHitbox();
        attackAfterlagTimer = ATTACK_AFTERLAG * attackRateModifier;

        if (currentState == MonsterState.attacking)
            currentState = MonsterState.idle;
    }


    private void FireProjectile()
    {
        acS.PlaySound("Enemy Shoot");
        if (monsterType == 1)
            projectileMS.SpawnProjectile(transform.position, 2);
        else if(monsterType == 2)
            projectileMS.SpawnProjectile(transform.position, 3);
    }

    public void Struck(float damage, Vector2 knockbackV)
    {
        if (struckTimer <= 0) // prevents multi-hits
        {
            acS.PlaySound("Enemy Struck");

            if ((playerS.GetLightMode() && monsterType == 1) || (!playerS.GetLightMode() && monsterType == 2) || (playerS.GetLightMode() && monsterType == 3) || (!playerS.GetLightMode() && monsterType == 4)) 
            {
                rb.velocity = knockbackV * 8f;
                health -= damage * 2f;
            }
            else
            {
                rb.velocity = knockbackV * 2f;
                health -= damage;
            }
            struckTimer = 1000000f;
            currentState = MonsterState.struck;
            if (health > 0)
            {
                monsterAS.SetToAnimation(AnimationState.moving); // WAS struck
                StartCoroutine(UnlockFromStruck());
            }
            else
            {
                Death();
            }
        }
    }

    IEnumerator UnlockFromStruck()
    {
        yield return new WaitForSeconds(0.1f);
        struckTimer = STRUCK_COOLDOWN;
        currentState = MonsterState.idle;
        monsterAS.SetToAnimation(AnimationState.moving); // WAS idle
    }

    public void Death()
    {
        acS.PlaySound("Enemy Dead");
        rb.constraints = RigidbodyConstraints2D.None;
        currentState = MonsterState.dead;
        monsterAS.SetToAnimation(AnimationState.death);
        gameObject.layer = 8;
        rb.velocity = new Vector2(Random.Range(-1f,1f), Random.Range(3f, 4f));
        if(monsterType == 1 || monsterType == 2)
            rb.AddTorque(Random.Range(-20f,20f));
        rb.gravityScale = 1f;
        StartCoroutine(DestroySelf());
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(4f);
        gameObject.SetActive(false);
    }
}
