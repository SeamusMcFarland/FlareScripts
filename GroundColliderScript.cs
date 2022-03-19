using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundColliderScript : MonoBehaviour
{
    List<GameObject> groundObjects = new List<GameObject>();
    bool grounded;
    AudioControllerScript acS;

    // Start is called before the first frame update
    void Start()
    {
        acS = GameObject.FindGameObjectWithTag("Audio Controller").GetComponent<AudioControllerScript>();

    }

    // Update is called once per frame
    void Update()
    {
        if (groundObjects.Count == 0)
            grounded = false;
        else
        {
            if (!grounded)
                acS.PlaySound("Landed");
            grounded = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Environment") && !groundObjects.Contains(collision.gameObject))
            groundObjects.Add(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Environment") && groundObjects.Contains(collision.gameObject))
            groundObjects.Remove(collision.gameObject);
    }

    public bool GetGrounded()
    {
        return grounded;
    }

}
