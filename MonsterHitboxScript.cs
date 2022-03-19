using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHitboxScript : MonoBehaviour
{
    List<GameObject> struckObjects = new List<GameObject>();
    List<GameObject> alreadyStruckObjects = new List<GameObject>(); // prevents multi-hits
    const float MELEE_DAMAGE = 2f;
    bool active;
    bool playerStrikable;

    public bool lightAttackType;

    PlayerScript playerS;

    // Start is called before the first frame update
    void Start()
    {
        playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active && playerStrikable)
        {
            if((playerS.GetLightMode() && lightAttackType) || (!playerS.GetLightMode() && !lightAttackType))
                playerS.Struck(MELEE_DAMAGE, GetKnockbackVector(playerS.gameObject));
            else
                playerS.Struck(MELEE_DAMAGE * 2f, GetKnockbackVector(playerS.gameObject) * 2f);
            active = false;
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

    public void EnableHitbox()
    {
        active = true;
    }

    public void DisableHitbox()
    {
        active = false;
    }
}
