using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public PlayerController player;

    public static GameManager instance = null;
    [SerializeField] Checkpoint lastCheckpoint = null;
    [SerializeField] public bool isTesting = false;

    List<Enemy> enemyList;

    void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Cursor.visible = false;
        enemyList = new List<Enemy>();
        Enemy[] temp = FindObjectsOfType<Enemy>();
        foreach(Enemy e in temp)
        {
            enemyList.Add(e);
        }

        
        //AkSoundEngine.PostEvent("Play_Blend_Amb_Bunker1", instance.gameObject);
    }

    void RespawnEnemies()
    {
        foreach(Enemy e in enemyList)
        {
            e.Respawn();
        }
    }

    public void Death()
    {
        if (!player.getIsAlive()) return;
        player.SetIsAlive(false);
        UIManager.instance.FadeIn(true);
        StartCoroutine("DeathCoroutine");
    }

    private void RespawnPlayer()
    {
        player.transform.position = lastCheckpoint.transform.position;
        player.SetIsAlive(true);
    }

    IEnumerator DeathCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        RespawnEnemies();
        RespawnPlayer();
        UIManager.instance.FadeIn(false);
    }

    public void SetNewCheckpoint(Checkpoint c)
    {
        if (c == lastCheckpoint) return; // pas sur que ça marche ?
        lastCheckpoint = c;
        print("New Checkpoint activated");
    }
}
