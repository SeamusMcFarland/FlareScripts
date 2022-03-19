using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBeamScript : MonoBehaviour
{
    const float MELEE_DAMAGE = 10000f;
    bool active;
    bool playerStrikable;

    public bool lightAttackType;

    PlayerScript playerS;

    float frameNormalizer;

    public GameObject spriteObject;

    // Start is called before the first frame update
    void Start()
    {
        playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        spriteObject.transform.localScale = new Vector2(Random.Range(0.85f,1.15f),1f);
        frameNormalizer = Time.deltaTime / 0.04f;
        if (active && playerStrikable)
        {
            if ((playerS.GetLightMode() && lightAttackType) || (!playerS.GetLightMode() && !lightAttackType))
                playerS.Struck(MELEE_DAMAGE * frameNormalizer, GetKnockbackVector(playerS.gameObject));
            else
                playerS.Struck(MELEE_DAMAGE * frameNormalizer * 2f, GetKnockbackVector(playerS.gameObject) * 2f);
        }
    }

    private Vector2 GetKnockbackVector(GameObject o)
    {
        return (new Vector2(o.transform.position.x - transform.position.x, o.transform.position.y - transform.position.y) / Vector2.Distance(o.transform.position, transform.position));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerStrikable = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerStrikable = false;
    }
}
