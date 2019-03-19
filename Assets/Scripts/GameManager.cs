using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public PlayerController player;

    public static GameManager instance = null;
    [SerializeField] Checkpoint lastCheckpoint = null;

    void Awake()
    {
        if (instance == null)

            instance = this;

        else if (instance != this)

            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void Death()
    {
        UIManager.instance.FadeIn(true);
        StartCoroutine("DeathCoroutine");
    }

    private void Respawn()
    {
        player.transform.position = lastCheckpoint.transform.position;
    }

    IEnumerator DeathCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        Respawn();
        UIManager.instance.FadeIn(false);
    }

    public void SetNewCheckpoint(Checkpoint c)
    {
        lastCheckpoint = c;
    }
}
