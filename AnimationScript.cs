using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    SpriteRenderer sr;
    private float frameTimer; // counts down to next animation frame change
    public AnimationState currentState;
    int currentSprite; // starts on 1 NOT 0
    public float idleFrameTime;
    public Sprite[] idleSprites;
    public float movingFrameTime;
    public Sprite[] movingSprites;
    public float attackingFrameTime;
    public Sprite[] attackingSprites;
    public float struckFrameTime;
    public Sprite[] struckSprites;
    public float deathFrameTime;
    public Sprite[] deathSprites;
    public float transformingFrameTime;
    public Sprite[] transformingSprites;
    public float jumpingFrameTime;
    public Sprite[] jumpingSprites;

    private void Awake()
    {
        currentSprite = 1;
        frameTimer = idleFrameTime;
        sr = GetComponent<SpriteRenderer>();
        currentState = AnimationState.idle;
    }

    private void Update()
    {
        if (currentState == AnimationState.hidden)
            sr.enabled = false;
        frameTimer -= Time.deltaTime;
        if(frameTimer <= 0)
            CheckAnimationChange();
        UpdateCurrentSprite();
    }

    private void UpdateCurrentSprite()
    {
        switch (currentState)
        {
            case AnimationState.idle:
                sr.sprite = idleSprites[currentSprite - 1];
                break;

            case AnimationState.moving:
                sr.sprite = movingSprites[currentSprite - 1];
                break;

            case AnimationState.attacking:
                sr.sprite = attackingSprites[currentSprite - 1];
                break;

            case AnimationState.struck:
                sr.sprite = struckSprites[currentSprite - 1];
                break;

            case AnimationState.death:
                sr.sprite = deathSprites[currentSprite - 1];
                break;

            case AnimationState.transforming:
                sr.sprite = transformingSprites[currentSprite - 1];
                break;

            case AnimationState.jumping:
                sr.sprite = jumpingSprites[currentSprite - 1];
                break;
        }
    }

    private void CheckAnimationChange()
    {
        switch (currentState)
        {
            case AnimationState.idle:
                frameTimer = idleFrameTime;
                if (currentSprite < idleSprites.Length)
                    currentSprite++;
                else
                    currentSprite = 1;
                break;

            case AnimationState.moving:
                frameTimer = movingFrameTime;
                if (currentSprite < movingSprites.Length)
                    currentSprite++;
                else
                    currentSprite = 1;
                break;

            case AnimationState.attacking:
                frameTimer = attackingFrameTime;
                if (currentSprite < attackingSprites.Length)
                    currentSprite++;
                break;

            case AnimationState.struck:
                frameTimer = struckFrameTime;
                if (currentSprite < struckSprites.Length)
                    currentSprite++;
                break;

            case AnimationState.death:
                frameTimer = deathFrameTime;
                if (currentSprite < deathSprites.Length)
                    currentSprite++;
                break;

            case AnimationState.hidden:
                //nothing to do here
                break;

            case AnimationState.transforming:
                frameTimer = transformingFrameTime;
                if (currentSprite < transformingSprites.Length)
                    currentSprite++;
                break;

            case AnimationState.jumping:
                frameTimer = jumpingFrameTime;
                if (currentSprite < jumpingSprites.Length)
                    currentSprite++;
                break;

            default:
                Debug.LogError("ERROR! INVALID ANIMATION STATE");
                break;
        }
    }

    public void FlipTo(bool isRight)
    {
        if (isRight)
            sr.flipX = true;
        else
            sr.flipX = false;
    }

    public void SetToAnimation(AnimationState anSt)
    {
        if (anSt != currentState) // disables animation resets if already on that animation state
        {
            if (anSt != AnimationState.hidden)
                sr.enabled = true;

            currentState = anSt;
            currentSprite = 1;

            switch (currentState)
            {
                case AnimationState.idle:
                    frameTimer = idleFrameTime;
                    break;

                case AnimationState.moving:
                    frameTimer = movingFrameTime;
                    break;

                case AnimationState.attacking:
                    frameTimer = attackingFrameTime;
                    break;

                case AnimationState.struck:
                    frameTimer = struckFrameTime;
                    break;

                case AnimationState.death:
                    frameTimer = deathFrameTime;
                    break;

                case AnimationState.hidden:
                    //nothing to do here
                    break;

                case AnimationState.transforming:
                    frameTimer = transformingFrameTime;
                    break;

                case AnimationState.jumping:
                    frameTimer = jumpingFrameTime;
                    break;

                default:
                    Debug.LogError("ERROR! INVALID ANIMATION STATE");
                    break;
            }
        }
    }

    public AnimationState GetAnimationState()
    {
        return currentState;
    }
}
