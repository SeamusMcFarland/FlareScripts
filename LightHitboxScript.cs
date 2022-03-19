using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHitboxScript : MonoBehaviour
{
    List<GameObject> struckObjects = new List<GameObject>();
    List<GameObject> alreadyStruckObjects = new List<GameObject>(); // prevents multi-hits
    const float MELEE_DAMAGE = 2f;
    bool active;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            foreach (GameObject o in struckObjects)
                if (o.CompareTag("Enemy") && !alreadyStruckObjects.Contains(o))
                {
                    alreadyStruckObjects.Add(o);
                    o.GetComponent<EnemyScript>().Struck(MELEE_DAMAGE, GetKnockbackVector(o));
                }
        }
    }

    private Vector2 GetKnockbackVector(GameObject o)
    {
        return (new Vector2(o.transform.position.x - transform.position.x, o.transform.position.y - transform.position.y) / Vector2.Distance(o.transform.position, transform.position));
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !struckObjects.Contains(collision.gameObject))
            struckObjects.Add(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && struckObjects.Contains(collision.gameObject))
            struckObjects.Remove(collision.gameObject);
    }

    public void EnableHitbox()
    {
        alreadyStruckObjects.Clear();
        active = true;
    }

    public void DisableHitbox()
    {
        active = false;
    }

}
