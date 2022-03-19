using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    const float PROJECTILE_SPEED = 10f;
    const float PROJECTILE_DAMAGE = 1f;

    AudioControllerScript audioS;
    const int AUDIO_NUM = 3;

    float xDiff;
    float yDiff;
    float rotation;
    Rigidbody2D rb;

    bool active;

    List<GameObject> struckObjects = new List<GameObject>();
    bool isColliding;
    SpriteRenderer sr;

    Vector2 savedPosition;

    bool isPlayer;

    PlayerScript playerS;

    public Sprite[] projectileSprites1;
    public Sprite[] projectileSprites2;
    public Sprite[] projectileSprites3;
    int currentSprite; // current frame in animation
    int currentType; // projectile type

    float animationTimer;
    const float ANIMATION_TIME = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();

        savedPosition = new Vector2(-200f, -200f);
        sr = GetComponent<SpriteRenderer>();
        active = false;
        rb = GetComponent<Rigidbody2D>();
        audioS = GameObject.FindGameObjectWithTag("Audio Controller").GetComponent<AudioControllerScript>();

        StartCoroutine(DelayDestroySelf());
    }

    IEnumerator DelayDestroySelf()
    {
        yield return new WaitForSeconds(0.01f);
        DestroySelf();
    }

    void Update()
    {
        animationTimer -= Time.deltaTime;
        CheckSprite();

        if (isPlayer)
        {
            foreach (GameObject o in struckObjects)
            {
                if (o.CompareTag("Environment") || o.CompareTag("Enemy"))
                {
                    if (o.CompareTag("Enemy"))
                        o.GetComponent<EnemyScript>().Struck(PROJECTILE_DAMAGE, GetKnockbackVector(o));
                    isColliding = true;
                }
            }
        }
        else
        {
            foreach (GameObject o in struckObjects)
            {
                if (o.CompareTag("Environment") || o.CompareTag("Player"))
                {
                    if (o.CompareTag("Player"))
                    {
                        if((currentType == 2 && playerS.GetLightMode()) || (currentType == 3 && !playerS.GetLightMode()))
                            playerS.Struck(PROJECTILE_DAMAGE * 2f, GetKnockbackVector(o) * 2f);
                        else
                            playerS.Struck(PROJECTILE_DAMAGE, GetKnockbackVector(o));
                    }
                    isColliding = true;
                }
            }
        }

        if (isColliding && active)
            DestroySelf();
    }

    private void CheckSprite()
    {
        if (animationTimer <= 0)
        {
            animationTimer = ANIMATION_TIME;
            switch (currentType)
            {
                case 1:
                    if (currentSprite + 1 < projectileSprites1.Length)
                        currentSprite++;
                    else
                        currentSprite = 0;
                    sr.sprite = projectileSprites1[currentSprite];
                    break;

                case 2:
                    if (currentSprite + 1 < projectileSprites2.Length)
                        currentSprite++;
                    else
                        currentSprite = 0;
                    sr.sprite = projectileSprites2[currentSprite];
                    break;

                case 3:
                    if (currentSprite + 1 < projectileSprites3.Length)
                        currentSprite++;
                    else
                        currentSprite = 0;
                    sr.sprite = projectileSprites3[currentSprite];
                    break;
            }
        }
    }

    private Vector2 GetKnockbackVector(GameObject o)
    {
        return (new Vector2(o.transform.position.x - transform.position.x, o.transform.position.y - transform.position.y) / Vector2.Distance(o.transform.position, transform.position));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Environment") || collision.CompareTag("Enemy") || collision.CompareTag("Player")) && !struckObjects.Contains(collision.gameObject))
            struckObjects.Add(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.CompareTag("Environment") || collision.CompareTag("Enemy") || collision.CompareTag("Player")) && struckObjects.Contains(collision.gameObject))
            struckObjects.Remove(collision.gameObject);
    }

    private void DestroySelf()
    {
        rb.velocity = new Vector2(0, 0);
        sr.enabled = false;
        gameObject.SetActive(false);
        //print("disabled at: " + Time.time);
        //audioS.PlaySound("Test Sound");
        active = false;
        //rb.MovePosition(new Vector2(400f, 400f));
    }

    public void Shoot(Vector2 pos, int type) // 1 for player, 2 for monster
    {
        currentType = type;
        if (type == 1)
            isPlayer = true;
        else
            isPlayer = false;
        
        gameObject.SetActive(true);
        isColliding = false;
        rb.velocity = new Vector2(0, 0);
        rb.MovePosition(pos);
        if(type == 1)
            PointTowards(pos, (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition));
        else
            PointTowards(pos, playerS.transform.position);
        active = true;
        StartCoroutine(DelayRenderer());
    }

    IEnumerator DelayRenderer()
    {
        yield return new WaitForSeconds(0.05f);
        rb.velocity = new Vector2(Mathf.Cos(rotation * Mathf.Deg2Rad), Mathf.Sin(rotation * Mathf.Deg2Rad)) * PROJECTILE_SPEED;
        sr.enabled = true;
    }

    void PointTowards(Vector2 pos, Vector2 targetPosition)
    {
        xDiff = targetPosition.x - pos.x;
        yDiff = targetPosition.y - pos.y;
        rotation = Mathf.Rad2Deg * Mathf.Atan2(yDiff, xDiff);
        rb.SetRotation(Quaternion.Euler(0, 0, rotation));
    }

    //void FixedUpdate()
    //{
    //    if (savedPosition.x != -200f)
    //    {
    //        rb.MovePosition(savedPosition);
    //        savedPosition = new Vector2(-200f, -200f);
    //    }
    //}



}
