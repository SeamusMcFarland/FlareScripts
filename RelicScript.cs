using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicScript : MonoBehaviour
{
    List<EnemyScript> enemyS = new List<EnemyScript>();
    public GameObject WinCanvas;
    AudioControllerScript acS;
    PlayerScript playerS;

    SpriteRenderer sr;
    public Sprite[] relicSprites;
    int currentSprite;
    float animationTimer;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerS = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("Enemy"))
            enemyS.Add(o.GetComponent<EnemyScript>());
        acS = GameObject.FindGameObjectWithTag("Audio Controller").GetComponent<AudioControllerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        animationTimer -= Time.deltaTime;
        if (animationTimer <= 0)
        {
            animationTimer = 0.2f;
            if (currentSprite + 1 < relicSprites.Length)
                currentSprite++;
            else
                currentSprite = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            WinGame();
    }

    public void WinGame()
    {
        playerS.SetHealth(10000f); // prevents post-win death
        foreach (EnemyScript eS in enemyS)
            eS.Death();
        acS.PlaySound("Win");
        WinCanvas.SetActive(true);
    }


}
