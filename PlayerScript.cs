using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    private bool lightMode; // Is the player currently the light creature or the dark creature? Starts OFF
    enum PlayerState { idle, attacking, struck, dead, transforming }; // specific to this creature // Moving is NOT a state for PlayerState
    private PlayerState currentState;

    public GameObject darkAO, lightAO; // light/dark animation game object
    private AnimationScript darkAS, lightAS; // each light/dark mode has its own animation script and game object
    private bool upPress, leftPress, downPress, rightPress, shiftPress, mousePress;

    private Rigidbody2D rb;

    private const float DARK_ACCELERATION = 3f;
    private const float MAX_DARK_SPEED = 3f;

    private const float LIGHT_ACCELERATION = 200f;
    private const float MAX_LIGHT_SPEED = 6f;
    private const float AIRBORN_ACCELERATION_MOD = 0.4f;
    private const float MAX_LIGHT_AIR_SPEED = 4f;
    private const float ATTACK_ACCELERATION_MOD = 0.2f;

    private GroundColliderScript gcS;

    private AudioControllerScript acS;

    private float transformTimer;
    private const float TRANSFORM_TIMER_MAX = 2f; // time before can transform again
    private const float TRANSFORM_AFTERLAG = 0.5f; // time before can act again after transforming

    private const float LIGHT_GRAVITY = 2.5f;
    private const float LIGHT_FASTFALL_GRAVITY = 5f;
    private float jumpTimer;
    private const float JUMP_HEIGHT = 13f;

    private float attackAfterlagTimer;
    private const float LIGHT_ATTACK_AFTERLAG = 0.05f;
    private const float DARK_ATTACK_AFTERLAG = 0.1f;
    private const float LIGHT_ATTACK_STARTUP = 0.1f;
    private const float DARK_ATTACK_STARTUP = 0.05f;
    private const float LIGHT_ATTACK_LENGTH = 0.5f;
    private const float DARK_ATTACK_LENGTH = 0.3f;
    private const float LIGHT_HITBOX_LENGTH = 0.1f;
    public ProjectileManagerScript projectileMS;
    public LightHitboxScript leftLHS;
    public LightHitboxScript rightLHS;
    private SpriteRenderer lightSR;

    private float frameNormalizer;

    private const float STRUCK_COOLDOWN = 0.5f;
    private float struckTimer;
    private float health;

    public GameObject loseCanvas;

    private float generalTimer;
    public Text loseTimeText;

    // Start is called before the first frame update
    void Start()
    {
        generalTimer = 0;
        health = 10f;
        gcS = GetComponentInChildren<GroundColliderScript>();
        acS = GameObject.FindGameObjectWithTag("Audio Controller").GetComponent<AudioControllerScript>();
        darkAS = darkAO.GetComponent<AnimationScript>();
        lightAS = lightAO.GetComponent<AnimationScript>();
        lightSR = lightAS.GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        lightAS.SetToAnimation(AnimationState.hidden);
        currentState = PlayerState.idle;
        transformTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //print("playerstate: " + currentState + " animation state: " + lightAS.GetAnimationState() + " at: " + Time.time);
        frameNormalizer = Time.deltaTime / 0.04f;

        CheckInputs();

        generalTimer += Time.deltaTime;
        transformTimer -= Time.deltaTime;
        struckTimer -= Time.deltaTime;
        jumpTimer -= Time.deltaTime;
        attackAfterlagTimer -= Time.deltaTime;
        if (lightMode)
            CheckLightGravity();

        switch (currentState)
        {
            case PlayerState.attacking:
                CheckTransform();
                if (lightMode)
                {
                    CheckLightMovement();
                    CheckLightJump();
                }
                else
                {
                    CheckDarkMovement();
                }
                break;

            case PlayerState.dead:
                lightAS.SetToAnimation(AnimationState.death);
                break;

            case PlayerState.idle:
                CheckTransform();
                CheckIdleAnimation();
                CheckAttack();
                if (lightMode)
                {
                    CheckLightMovement();
                    CheckLightJump();
                }
                else
                {
                    CheckDarkMovement();
                }
                break;

            case PlayerState.struck:
                if(lightMode)
                    lightAS.SetToAnimation(AnimationState.struck);
                else
                    darkAS.SetToAnimation(AnimationState.struck);
                break;

            case PlayerState.transforming:

                break;

            default:
                Debug.LogError("WARNING! INVALID PLAYERSTATE IN UPDATE!");
                break;
        }

    }

    private void CheckIdleAnimation()
    {
        if (lightMode)
        {
            if (!gcS.GetGrounded())
                lightAS.SetToAnimation(AnimationState.jumping);
            else if (((leftPress || rightPress) && lightMode) || ((leftPress || rightPress || upPress || downPress) && !lightMode))
                lightAS.SetToAnimation(AnimationState.moving);
            else
                lightAS.SetToAnimation(AnimationState.idle);
        }
        else
        {
            if (((leftPress || rightPress) && lightMode) || ((leftPress || rightPress || upPress || downPress) && !lightMode))
                darkAS.SetToAnimation(AnimationState.moving);
            else
                darkAS.SetToAnimation(AnimationState.idle);
        }
    }

    private void CheckAttack()
    {
        if (mousePress && attackAfterlagTimer <= 0)
        {
            attackAfterlagTimer = 100f; // locks until set to constant
            if (lightMode)
                LightAttack();
            else
                DarkAttack();
        }
    }

    private void LightAttack()
    {
        if(gcS.GetGrounded())
            rb.velocity = new Vector2(rb.velocity.x/2f, rb.velocity.y);
        lightAS.SetToAnimation(AnimationState.attacking);
        currentState = PlayerState.attacking;
        StartCoroutine(UnlockFromAttack());
    }

    private void DarkAttack()
    {
        acS.PlaySound("Dark Shoot");
        darkAS.SetToAnimation(AnimationState.attacking);
        currentState = PlayerState.attacking;
        StartCoroutine(UnlockFromAttack());
    }

    IEnumerator UnlockFromAttack()
    {
        if (lightMode)
        {
            yield return new WaitForSeconds(LIGHT_ATTACK_STARTUP);
            if (currentState == PlayerState.attacking)
            {
                acS.PlaySound("Light Attack");
                if (lightSR.flipX)
                    leftLHS.EnableHitbox();
                else
                    rightLHS.EnableHitbox();
            }
            yield return new WaitForSeconds(LIGHT_ATTACK_LENGTH);
            leftLHS.DisableHitbox();
            rightLHS.DisableHitbox();
            attackAfterlagTimer = LIGHT_ATTACK_AFTERLAG;
        }
        else
        {
            yield return new WaitForSeconds(DARK_ATTACK_STARTUP);
            if (currentState == PlayerState.attacking)
                FireProjectile();
            yield return new WaitForSeconds(DARK_ATTACK_LENGTH);
            attackAfterlagTimer = DARK_ATTACK_AFTERLAG;
        }
        if (currentState == PlayerState.attacking)
            currentState = PlayerState.idle;
    }

    private void FireProjectile()
    {
        projectileMS.SpawnProjectile(rb.position, 1);
    }

    private void CheckInputs()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            upPress = true;
        else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space))
            upPress = false;
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            leftPress = true;
        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            leftPress = false;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            downPress = true;
        else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
            downPress = false;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            rightPress = true;
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            rightPress = false;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            shiftPress = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            shiftPress = false;
        if (Input.GetMouseButtonDown(0))
            mousePress = true;
        else if (Input.GetMouseButtonUp(0))
            mousePress = false;
    }

    private void CheckTransform()
    {
        if (shiftPress && transformTimer <= 0)
            TransformSelf();
    }

    private void TransformSelf()
    {
        currentState = PlayerState.transforming;
        acS.PlaySound("Transform");
        if (lightMode)
            SwitchToDark();
        else
            SwitchToLight();
    }


    private void CheckDarkMovement()
    {
        if (upPress && !downPress && rb.velocity.y < MAX_DARK_SPEED)
            DarkMoveUp();
        else if (!upPress && downPress && rb.velocity.y > -MAX_DARK_SPEED)
            DarkMoveDown();
        if (rightPress && !leftPress && rb.velocity.x < MAX_DARK_SPEED)
            DarkMoveRight();
        else if (!rightPress && leftPress && rb.velocity.x > -MAX_DARK_SPEED)
            DarkMoveLeft();
    }

    private void DarkMoveUp()
    {
        rb.AddForce(new Vector2(0, DARK_ACCELERATION * frameNormalizer));
    }

    private void DarkMoveLeft()
    {
        rb.AddForce(new Vector2(-DARK_ACCELERATION * frameNormalizer, 0));
        darkAS.FlipTo(true);
    }

    private void DarkMoveDown()
    {
        rb.AddForce(new Vector2(0, -DARK_ACCELERATION * frameNormalizer));
    }

    private void DarkMoveRight()
    {
        rb.AddForce(new Vector2(DARK_ACCELERATION * frameNormalizer, 0));
        darkAS.FlipTo(false);
    }

    private void CheckLightMovement()
    {
        if (gcS.GetGrounded())
        {
            if (rightPress && !leftPress && rb.velocity.x < MAX_LIGHT_SPEED)
                LightMoveRight();
            else if (!rightPress && leftPress && rb.velocity.x > -MAX_LIGHT_SPEED)
                LightMoveLeft();
        }
        else
        {
            if (rightPress && !leftPress && rb.velocity.x < MAX_LIGHT_AIR_SPEED)
                LightMoveRight();
            else if (!rightPress && leftPress && rb.velocity.x > -MAX_LIGHT_AIR_SPEED)
                LightMoveLeft();
        }
    }

    private void LightMoveLeft()
    {
        if (currentState != PlayerState.attacking)
        {
            if (gcS.GetGrounded())
                rb.AddForce(new Vector2(-LIGHT_ACCELERATION * frameNormalizer, 0));
            else
                rb.AddForce(new Vector2(-LIGHT_ACCELERATION * AIRBORN_ACCELERATION_MOD * frameNormalizer, 0));
            lightAS.FlipTo(true);
        }
        else
        {
            if (gcS.GetGrounded())
                rb.AddForce(new Vector2(-LIGHT_ACCELERATION * ATTACK_ACCELERATION_MOD * frameNormalizer, 0));
            else
                rb.AddForce(new Vector2(-LIGHT_ACCELERATION * AIRBORN_ACCELERATION_MOD * frameNormalizer, 0));
        }
    }

    private void LightMoveRight()
    {
        if (currentState != PlayerState.attacking)
        {
            if (gcS.GetGrounded())
                rb.AddForce(new Vector2(LIGHT_ACCELERATION * frameNormalizer, 0));
            else
                rb.AddForce(new Vector2(LIGHT_ACCELERATION * AIRBORN_ACCELERATION_MOD * frameNormalizer, 0));
            lightAS.FlipTo(false);
        }
        else
        {
            if (gcS.GetGrounded())
                rb.AddForce(new Vector2(LIGHT_ACCELERATION * ATTACK_ACCELERATION_MOD * frameNormalizer, 0));
            else
                rb.AddForce(new Vector2(LIGHT_ACCELERATION * AIRBORN_ACCELERATION_MOD * ATTACK_ACCELERATION_MOD * frameNormalizer, 0));
        }
    }

    private void CheckLightJump()
    {
        if (gcS.GetGrounded() && upPress && jumpTimer <= 0)
            LightJump();
    }

    private void LightJump()
    {
        jumpTimer = 0.05f;
        rb.velocity = new Vector2(rb.velocity.x, JUMP_HEIGHT);
    }

    private void CheckLightGravity()
    {
        if (downPress && (currentState == PlayerState.attacking || currentState == PlayerState.idle))
            rb.gravityScale = LIGHT_FASTFALL_GRAVITY;
        else
            rb.gravityScale = LIGHT_GRAVITY;
    }

    private void SwitchToDark()
    {
        darkAS.SetToAnimation(AnimationState.transforming);
        lightAS.SetToAnimation(AnimationState.hidden);
        rb.mass = 0.5f;
        rb.drag = 1f;
        rb.gravityScale = 0.05f;
        lightMode = false;
        transformTimer = TRANSFORM_TIMER_MAX;
        StartCoroutine(UnlockFromTransform());
    }

    private void SwitchToLight()
    {
        darkAS.SetToAnimation(AnimationState.hidden);
        lightAS.SetToAnimation(AnimationState.transforming);
        rb.mass = 1f;
        rb.drag = 0;
        rb.gravityScale = LIGHT_GRAVITY;
        lightMode = true;
        transformTimer = TRANSFORM_TIMER_MAX;
        StartCoroutine(UnlockFromTransform());
    }

    IEnumerator UnlockFromTransform()
    {
        yield return new WaitForSeconds(0.01f); // delayed frame force
        if (lightMode)
        {
            darkAS.SetToAnimation(AnimationState.hidden);
            lightAS.SetToAnimation(AnimationState.transforming);
        }
        else
        {
            darkAS.SetToAnimation(AnimationState.transforming);
            lightAS.SetToAnimation(AnimationState.hidden);
        }
        yield return new WaitForSeconds(TRANSFORM_AFTERLAG);
        if (currentState == PlayerState.transforming)
            currentState = PlayerState.idle;
        if (lightMode)
        {
            if (lightAS.GetAnimationState() == AnimationState.transforming)
                lightAS.SetToAnimation(AnimationState.idle);
        }
        else
        {
            if (darkAS.GetAnimationState() == AnimationState.transforming)
                darkAS.SetToAnimation(AnimationState.idle);
        }
    }

    public void Struck(float damage, Vector2 knockbackV)
    {
        if (struckTimer <= 0) // prevents multi-hits
        {
            if(lightMode)
                rb.velocity = knockbackV * 3f;
            else
                rb.velocity = knockbackV;
            health -= damage;
            struckTimer = 1000000f;
            leftLHS.DisableHitbox();
            rightLHS.DisableHitbox();
            currentState = PlayerState.struck;
            if (health > 0)
            {
                if (lightMode)
                {
                    acS.PlaySound("Light Struck Weak");
                    lightAS.SetToAnimation(AnimationState.struck);
                }
                else
                {
                    acS.PlaySound("Dark Struck");
                    darkAS.SetToAnimation(AnimationState.struck);
                }
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
        yield return new WaitForSeconds(0.3f);
        struckTimer = STRUCK_COOLDOWN;
        currentState = PlayerState.idle;
        if (lightMode)
            lightAS.SetToAnimation(AnimationState.idle);
        else
            darkAS.SetToAnimation(AnimationState.idle);
    }

    private void Death()
    {
        currentState = PlayerState.dead;
        acS.PlaySound("Player Death");
        if (lightMode)
            lightAS.SetToAnimation(AnimationState.death);
        else
            darkAS.SetToAnimation(AnimationState.death);
        StartCoroutine(DelayLoseGame());
    }

    IEnumerator DelayLoseGame()
    {
        yield return new WaitForSeconds(1f);
        LoseGame();
    }

    public void LoseGame()
    {
        loseTimeText.text = "" + ((int)generalTimer);
        loseCanvas.SetActive(true);
    }

    public bool GetLightMode()
    {
        return lightMode;
    }

    public void SetHealth(float h)
    {
        health = h;
    }

}
