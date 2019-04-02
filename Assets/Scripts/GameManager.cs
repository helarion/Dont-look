﻿using Rewired;
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

    [Header("Layers")]
    [SerializeField] LayerMask wallsAndMobsLayer;

    [HideInInspector] public Player controls; // The Rewired Player

    Vector3 originalPos;
    bool isPaused = false;

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
        controls = ReInput.players.GetPlayer(0);
        ResumeGame();
        originalPos = mainCamera.transform.localPosition;
        Cursor.visible = false;
        enemyList = new List<Enemy>();
        Enemy[] temp = FindObjectsOfType<Enemy>();
        foreach(Enemy e in temp)
        {
            enemyList.Add(e);
        }
    }

    private void Update()
    {
        // PAUSE HANDLER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
        if (isPaused) return;
        CheckShake();
    }

    // Shaking screen for duration set previously
    void CheckShake()
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

    // RESPAWN CHAQUE ENNEMI
    void RespawnEnemies()
    {
        foreach(Enemy e in enemyList)
        {
            e.Respawn();
        }
    }

    // TUE LE JOUEUR
    public void Death()
    {
        if (!player.getIsAlive()) return;
        player.SetIsAlive(false);
        UIManager.instance.FadeDeath(true);
        StartCoroutine("DeathCoroutine");
    }

    // RESPAWN LE JOUEUR
    private void RespawnPlayer()
    {
        player.transform.position = lastCheckpoint.transform.position;
        player.SetIsAlive(true);
    }

    // COROUTINE DE FADE OUT / IN DE LA MORT
    IEnumerator DeathCoroutine()
    {
        while (UIManager.instance.isFading)
            yield return new WaitForSeconds(0.1f);
        RespawnEnemies();
        RespawnPlayer();
        UIManager.instance.FadeDeath(false);
    }

    // SAUVEGARDE LE NOUVEAU CHECKPOINT : POINT DE RESPAWN POUR LE JOUEUR 
    public void SetNewCheckpoint(Checkpoint c)
    {
        if (c == lastCheckpoint) return;
        lastCheckpoint = c;
        print("New Checkpoint activated");
    }

    // SHAKESCREEN POUR LA DUREE ENTREE
    public void ShakeScreen(float duration)
    {
        shakeDuration = duration;
    }

    // SHAKESCREEN A VALEUR PROGRESSIVE /// PAS ENCORE FONCTIONNEL
    public void ProgressiveShake(float duration)
    {
        float savedAmount = shakeAmount;
        shakeAmount = 0;
        ShakeScreen(duration);
        StartCoroutine("ShakeCoroutine", duration);
        shakeAmount = savedAmount;
    }

    // SHAKESCREEN PROGRESSIF COROUTINE /// PAS ENCORE FONCTIONNEL
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
    
    // BOUGER LA CAMERA
    public void MoveCamera(Vector3 newPos)
    {
        originalPos = Vector3.Lerp(originalPos, newPos, cameraSpeed);
    }

    public void RotateCamera(Quaternion newRotate)
    {
        mainCamera.transform.localRotation = Quaternion.Slerp(mainCamera.transform.localRotation, newRotate, cameraSpeed);
    }

    public LayerMask getWallsAndMobsLayer()
    {
        return wallsAndMobsLayer;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        isPaused = true;
        UIManager.instance.pausePanel.SetActive(true);
        //UIManager.instance.FadePause(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        isPaused = false;
        UIManager.instance.pausePanel.SetActive(false);
        //UIManager.instance.FadePause(false);
    }

    public bool GetIsPaused()
    {
        return isPaused;
    }
}
