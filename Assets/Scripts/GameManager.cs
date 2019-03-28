using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera mainCamera;
    public PlayerController player;

    [HideInInspector]public static GameManager instance = null;
    List<Enemy> enemyList;

    [SerializeField] float cameraSpeed=3;

    [Header("Debug")]
    [SerializeField] Checkpoint lastCheckpoint = null;
    [SerializeField] public bool isTesting = false;

    [Header("ScreenShake")]
    [SerializeField] float shakeDuration = 0f;
    [SerializeField] float shakeAmount = 0.7f;
    [SerializeField] float decreaseFactor = 1.0f;
    [SerializeField] float maxValue = 0.1f;

    Vector3 originalPos;

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
        originalPos = mainCamera.transform.localPosition;

        Cursor.visible = false;
        enemyList = new List<Enemy>();
        Enemy[] temp = FindObjectsOfType<Enemy>();
        foreach(Enemy e in temp)
        {
            enemyList.Add(e);
        }

        AkSoundEngine.PostEvent("Play_Blend_Amb_Bunker1", instance.gameObject);
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            mainCamera.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            mainCamera.transform.localPosition = originalPos;
        }
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
        if (c == lastCheckpoint) return;
        lastCheckpoint = c;
        print("New Checkpoint activated");
    }

    public void ShakeScreen(float duration)
    {
        shakeDuration = duration;
    }

    public void ProgressiveShake(float duration)
    {
        float savedAmount = shakeAmount;
        shakeAmount = 0;
        ShakeScreen(duration);
        StartCoroutine("ShakeCoroutine", duration);
        shakeAmount = savedAmount;
    }

    IEnumerator ShakeCoroutine(float maxTime)
    {
        float time = 0;
        float update = maxValue *(0.01f*maxTime);
        while (time<maxTime)
        {
            if (shakeAmount < maxValue) shakeAmount += update;
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }
    
    public void MoveCamera(Vector3 newPos)
    {
        originalPos = Vector3.Lerp(originalPos, newPos, cameraSpeed);
    }
}
