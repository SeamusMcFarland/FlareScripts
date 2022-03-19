using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManagerScript : MonoBehaviour
{
    public List<ProjectileScript> projectileS = new List<ProjectileScript>();
    int current;


    // Start is called before the first frame update
    void Start()
    {
        current = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Projectile"))
            projectileS.Add(obj.GetComponent<ProjectileScript>());
    }

    public void SpawnProjectile(Vector2 pos, int type)
    {
            projectileS[current].Shoot(pos, type);
            if (current < projectileS.Count - 1)
                current++;
            else
                current = 0;
        
    }

}
