using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private float frameNormalizer;
    private float speed;
    private float acceleration;
    private bool moving;

    // Start is called before the first frame update
    void Start()
    {
        moving = true;
        speed = 0.01f;
        acceleration = 0.0001f;
    }

    // Update is called once per frame
    void Update()
    {
        frameNormalizer = Time.deltaTime / 0.04f;
        if (moving)
        {
            if (transform.position.x > 168f)
                moving = false;
            speed += acceleration * frameNormalizer;
            transform.position = new Vector3(transform.position.x + speed * frameNormalizer, transform.position.y, -10f);
        }
    }
}
